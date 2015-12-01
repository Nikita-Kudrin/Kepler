﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Timers;
using AutoMapper.Internal;
using Kepler.Common.CommunicationContracts;
using Kepler.Common.Error;
using Kepler.Common.Models;
using Kepler.Common.Models.Common;
using Kepler.Common.Repository;
using Kepler.Service.RestWorkerClient;
using RestSharp;
using Timer = System.Timers.Timer;

namespace Kepler.Service.Core
{
    public class BuildExecutor
    {
        private static BuildExecutor _executor;
        private static Timer _sendScreenShotsForProcessingTimer;
        private static Timer _updateObjectStatusesTimer;
        private static Timer _checkWorkersTimer;
        public static string KeplerServiceUrl { get; set; }

        static BuildExecutor()
        {
            GetExecutor();
        }


        public static BuildExecutor GetExecutor()
        {
            _executor = _executor ?? new BuildExecutor();
            return _executor;
        }

        private BuildExecutor()
        {
            KeplerServiceUrl = new KeplerService().GetKeplerServiceUrl();
            CheckWorkersAvailability(null, null);
            UpdateKeplerServiceUrlOnWorkers();
            UpdateDiffImagePath();

            _sendScreenShotsForProcessingTimer = new Timer();
            _sendScreenShotsForProcessingTimer.Interval = 5000; //every 5 sec
            _sendScreenShotsForProcessingTimer.Elapsed += SendComparisonInfoToWorkers;
            _sendScreenShotsForProcessingTimer.Enabled = true;

            _updateObjectStatusesTimer = new Timer();
            _updateObjectStatusesTimer.Interval = 15000;
            _updateObjectStatusesTimer.Elapsed += UpdateObjectsStatuses;
            _updateObjectStatusesTimer.Enabled = true;

            _checkWorkersTimer = new Timer();
            _checkWorkersTimer.Interval = 60000;
            _checkWorkersTimer.Elapsed += CheckWorkersAvailability;
            _checkWorkersTimer.Enabled = true;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private void UpdateObjectsStatuses(object sender, ElapsedEventArgs eventArgs)
        {
            ObjectStatusUpdater.UpdateAllObjectStatusesToActual();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CheckWorkersAvailability(object sender, ElapsedEventArgs eventArgs)
        {
            var workers = ImageWorkerRepository.Instance.FindAll().ToList();

            if (workers == null || !workers.Any())
                return;

            foreach (var imageWorker in workers)
            {
                try
                {
                    var restImageProcessorClient = new RestImageProcessorClient(imageWorker.WorkerServiceUrl);
                    restImageProcessorClient.SetKeplerServiceUrl();

                    imageWorker.WorkerStatus = ImageWorker.StatusOfWorker.Available;
                }
                catch (Exception ex)
                {
                    ErrorMessageRepository.Instance.Insert(new ErrorMessage()
                    {
                        ExceptionMessage = $"Image worker: '{imageWorker.Name}' is unavailable. {ex.Message}"
                    });
                    imageWorker.WorkerStatus = ImageWorker.StatusOfWorker.Offline;
                }
                finally
                {
                    ImageWorkerRepository.Instance.UpdateAndFlashChanges(imageWorker);
                }
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SendComparisonInfoToWorkers(object sender, ElapsedEventArgs eventArgs)
        {
            var workers = ImageWorkerRepository.Instance.FindAll()
                .Where(worker => worker.WorkerStatus == ImageWorker.StatusOfWorker.Available).ToList();

            var buildInQueue = BuildRepository.Instance.GetInQueueBuilds().FirstOrDefault();

            if (buildInQueue == null)
                return;

            var screenShots = ScreenShotRepository.Instance.GetInQueueScreenShotsForBuild(buildInQueue.Id);
            screenShots.Each(item => item.Status = ObjectStatus.InProgress);
            ScreenShotRepository.Instance.UpdateAndFlashChanges(screenShots);
            UpdateObjectsStatuses(this, null);

            var imageComparisonContainers = ConvertScreenShotsToImageComparison(screenShots).ToList();

            if (workers.Count == 0)
                return;

            // split all screenshots for comparison uniformly for all workers
            var imgComparisonPerWorker = imageComparisonContainers.Count()/workers.Count();
            if (imgComparisonPerWorker == 0)
                imgComparisonPerWorker = imageComparisonContainers.Count();


            var workerIndex = 0;
            while (imageComparisonContainers.Any())
            {
                var jsonMessage = new ImageComparisonContract()
                {
                    ImageComparisonList = imageComparisonContainers.Take(imgComparisonPerWorker).ToList()
                };

                var requestIsSuccessfull = true;
                try
                {
                    var client = new RestClient(workers[workerIndex++].WorkerServiceUrl);
                    var request = new RestRequest("AddImagesForDiffGeneration", Method.POST);

                    request.RequestFormat = DataFormat.Json;
                    request.AddJsonBody(jsonMessage);

                    var response = client.Execute(request);
                    var responseErrorMessage = RestImageProcessorClient.GetResponseErrorMessage(response);

                    if (!string.IsNullOrEmpty(responseErrorMessage))
                        throw new WebException(responseErrorMessage);
                }
                catch (Exception ex)
                {
                    requestIsSuccessfull = false;

                    ErrorMessageRepository.Instance.Insert(new ErrorMessage()
                    {
                        ExceptionMessage =
                            $"Error happend when process is tried to send images for comparison to worker: '{workers[workerIndex++].Name}'.  {ex.Message}"
                    });
                }

                if (requestIsSuccessfull)
                {
                    if (imageComparisonContainers.Count() < imgComparisonPerWorker)
                        imageComparisonContainers.Clear();
                    else
                    {
                        imageComparisonContainers.RemoveRange(0, imgComparisonPerWorker);
                    }
                }

                if (workerIndex == workers.Count)
                    workerIndex = 0;
            }
        }


        /// <summary>
        /// Iterate through all newly imported screenshots and create for them image comparison info object (for future diff processing)
        /// </summary>
        /// <param name="newScreenShots"></param>
        /// <returns>List of image comparison info, that will be used in diff processing service</returns>
        public IEnumerable<ImageComparisonInfo> ConvertScreenShotsToImageComparison(
            IEnumerable<ScreenShot> newScreenShots)
        {
            var imagesComparisonContainer = new List<ImageComparisonInfo>();

            // group all screenshots by baseline
            var groupedNewScreenShots = newScreenShots.GroupBy(item => item.BaseLineId);

            // Iterate through each group of baseline
            foreach (var newBaselineScreenShot in groupedNewScreenShots)
            {
                var oldPassedBaselineScreenShots =
                    ScreenShotRepository.Instance.GetBaselineScreenShots(newBaselineScreenShot.Key);

                var newScreenShotsForProcessing = newBaselineScreenShot.AsEnumerable().ToList();

                imagesComparisonContainer.AddRange(GenerateImageComparison(newScreenShotsForProcessing,
                    oldPassedBaselineScreenShots.ToList()));
            }

            return imagesComparisonContainer;
        }

        /// <summary>
        /// Analyze two list of screenshots from the same baseline: new (In queue) and old (Passed)
        /// </summary>
        /// <param name="newScreenShotsForProcessing"></param>
        /// <param name="oldPassedBaselineScreenShots"></param>
        /// <returns>List of image comparison for diff generating</returns>
        private IEnumerable<ImageComparisonInfo> GenerateImageComparison(List<ScreenShot> newScreenShotsForProcessing,
            List<ScreenShot> oldPassedBaselineScreenShots)
        {
            var resultImagesForComparison = new List<ImageComparisonInfo>();

            foreach (var newScreenShot in newScreenShotsForProcessing)
            {
                var oldScreenShot = oldPassedBaselineScreenShots.Find(oldScreen => oldScreen.Name == newScreenShot.Name);

                ImageComparisonInfo imageComparison = null;

                // if there is no old screenshots => it's first build and first uploaded screenshots.
                // Just set screenshots as passed
                if (oldScreenShot == null)
                {
                    newScreenShot.Status = ObjectStatus.Passed;
                    newScreenShot.BaseLineImagePath = newScreenShot.ImagePath;
                    newScreenShot.IsLastPassed = true;

                    var screenShotPreviewImagePath = newScreenShot.PreviewImagePath ?? "";

                    imageComparison = new ImageComparisonInfo()
                    {
                        ScreenShotName = newScreenShot.Name,
                        LastPassedScreenShotId = newScreenShot.Id,
                        FirstImagePath = newScreenShot.ImagePath,
                        FirstPreviewPath = screenShotPreviewImagePath,
                        SecondImagePath = newScreenShot.ImagePath,
                        SecondPreviewPath = screenShotPreviewImagePath,
                        DiffImagePath = newScreenShot.DiffImagePath,
                        DiffPreviewPath = newScreenShot.DiffPreviewPath,
                        ScreenShotId = newScreenShot.Id,
                    };
                }
                else
                {
                    imageComparison = new ImageComparisonInfo()
                    {
                        ScreenShotName = newScreenShot.Name,
                        LastPassedScreenShotId = oldScreenShot.Id,
                        FirstImagePath = oldScreenShot.ImagePath,
                        FirstPreviewPath = oldScreenShot.PreviewImagePath,
                        SecondImagePath = newScreenShot.ImagePath,
                        SecondPreviewPath = newScreenShot.PreviewImagePath,
                        DiffImagePath = newScreenShot.DiffImagePath,
                        DiffPreviewPath = newScreenShot.DiffPreviewPath,
                        ScreenShotId = newScreenShot.Id,
                    };

                    newScreenShot.BaseLineImagePath = oldScreenShot.ImagePath;
                }
                resultImagesForComparison.Add(imageComparison);
                ScreenShotRepository.Instance.UpdateAndFlashChanges(newScreenShot);
            }

            return resultImagesForComparison;
        }

        public void UpdateKeplerServiceUrlOnWorkers()
        {
            var workers = ImageWorkerRepository.Instance.FindAll()
                .Where(worker => worker.WorkerStatus == ImageWorker.StatusOfWorker.Available).ToList();

            foreach (var imageWorker in workers)
            {
                var restImageProcessorClient = new RestImageProcessorClient(imageWorker.WorkerServiceUrl);
                restImageProcessorClient.SetKeplerServiceUrl();
            }
        }

        public void UpdateDiffImagePath()
        {
            var keplerService = new KeplerService();

            var diffImageSavingPath = keplerService.GetDiffImageSavingPath();
            var previewImageSavingPath = keplerService.GetPreviewSavingPath();

            if (string.IsNullOrEmpty(diffImageSavingPath))
                return;

            if (!Directory.Exists(diffImageSavingPath))
            {
                try
                {
                    Directory.CreateDirectory(diffImageSavingPath);
                }
                catch (Exception ex)
                {
                    ErrorMessageRepository.Instance.Insert(new ErrorMessage() {ExceptionMessage = ex.Message});
                }
            }

            if (!Directory.Exists(previewImageSavingPath))
            {
                try
                {
                    Directory.CreateDirectory(previewImageSavingPath);
                }
                catch (Exception ex)
                {
                    ErrorMessageRepository.Instance.Insert(new ErrorMessage() {ExceptionMessage = ex.Message});
                }
            }
        }
    }
}