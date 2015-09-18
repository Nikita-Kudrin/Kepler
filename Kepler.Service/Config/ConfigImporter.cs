﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Kepler.Common.Error;
using Kepler.Core;
using Kepler.Core.Common;
using Kepler.Models;
using Kepler.Service.Config;
using Newtonsoft.Json;

namespace Kepler.Service
{
    public class ConfigImporter
    {
        private ConfigMapper mapper = new ConfigMapper();

        /// <summary>
        /// Import json config file
        /// </summary>
        /// <param name="jsonConfig"></param>
        /// <returns>Return empty string, if import was 'Ok', otherwise - error message</returns>
        public string ImportConfig(string jsonConfig)
        {
            TestImportConfig importedConfig;
            try
            {
                importedConfig = JsonConvert.DeserializeObject<TestImportConfig>(jsonConfig);
            }
            catch (Exception ex)
            {
                return new ErrorMessage() {Code = ErrorMessage.ErorCode.ParsingFileError, ExceptionMessage = ex.Message}.ToString();
            }

            var validationErrorMessage = ValidateImportedConfigObjects(importedConfig);
            if (validationErrorMessage != "")
                return validationErrorMessage;

            var mappedProjects = mapper.GetProjects(importedConfig.Projects).ToList();

            try
            {
                mappedProjects = BindImportedProjectWithExistedInDB(mappedProjects);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            var builds = BindImportedProjectWithBuilds(mappedProjects);
            var assemblies = BindTestAssembliesWithBuilds(importedConfig, mappedProjects);
            assemblies = BindTestSuitesWithAssemblies(importedConfig, builds, assemblies);
            BindTestCasesWithSuites(importedConfig, assemblies);
            BindScreenshotsWithTestCases(importedConfig, assemblies);

            return "";
        }


        private string ValidateImportedConfigObjects(TestImportConfig importedConfig)
        {
            var mapper = new ConfigMapper();
            var mappedProjects = mapper.GetProjects(importedConfig.Projects).ToList();

            // validate projects
            var validationMessage = ConfigValidator.ValidateProjects(mappedProjects);
            if (validationMessage != "")
                return validationMessage;

            // validate test assemblies
            foreach (var project in importedConfig.Projects)
            {
                var assemblies = mapper.GetAssemblies(project.TestAssemblies);
                validationMessage = ConfigValidator.ValidateTestAssemblies(assemblies.ToList());

                if (validationMessage != "")
                    return validationMessage;
            }


            foreach (var project in importedConfig.Projects)
            {
                //validate suites
                foreach (var testAssemblyConfig in project.TestAssemblies)
                {
                    var items = mapper.GetSuites(testAssemblyConfig.TestSuites);
                    validationMessage = ConfigValidator.ValidateTestSuites(items.ToList());

                    if (validationMessage != "")
                        return validationMessage;


                    //validate cases
                    foreach (var testSuite in testAssemblyConfig.TestSuites)
                    {
                        var cases = mapper.GetCases(testSuite.TestCases);
                        validationMessage = ConfigValidator.ValidateTestCases(cases.ToList());

                        if (validationMessage != "")
                            return validationMessage;


                        //validate screenshots
                        foreach (var testCase in testSuite.TestCases)
                        {
                            validationMessage = ConfigValidator.ValidateScreenshots(testCase.ScreenShots);

                            if (validationMessage != "")
                                return validationMessage;
                        }
                    }
                }
            }

            return "";
        }


        private List<Project> BindImportedProjectWithExistedInDB(List<Project> mappedProjects)
        {
            for (int index = 0; index < mappedProjects.Count(); index++)
            {
                var project = mappedProjects[index];

                var projectName = project.Name.Trim();
                var projectsFromDb = ProjectRepository.Instance.Find(projectName);

                if (projectsFromDb.Count() == 0 || projectsFromDb.Count() > 1)
                    throw new Exception(new ErrorMessage()
                    {
                        Code = ErrorMessage.ErorCode.ObjectNotFoundInDb,
                        ExceptionMessage = $"Project '{projectName}' wasn't found in database (or there are more than 1 of them)"
                    }.ToString());

                mappedProjects[index] = projectsFromDb.FirstOrDefault();
            }

            return mappedProjects;
        }

        /// <summary>
        /// Create new build for each project and save them in DB
        /// </summary>
        /// <param name="mappedProjects"></param>
        /// <returns>List new bounded builds</returns>
        private List<Build> BindImportedProjectWithBuilds(List<Project> mappedProjects)
        {
            var builds = new List<Build>();

            // create new build for each project. save in db

            //  for (int index = 0; index < mappedProjects.Count; index ++)
            foreach (var mappedProject in mappedProjects)
            {
                var build = new Build() {Status = ObjectStatus.InQueue, ProjectId = mappedProject.Id};

                BuildRepository.Instance.Insert(build);
                builds.Add(build);

                mappedProject.Builds.Add(build.Id, build);
                mappedProject.LatestBuildId = build.Id;

                ProjectRepository.Instance.Update(mappedProject);
                ProjectRepository.Instance.FlushChanges();
            }

            return builds;
        }

        /// <summary>
        /// Create new test assemblies. Bind each assembly with corresponding build and project. Save assembly in DB
        /// </summary>
        /// <param name="importedConfig"></param>
        /// <param name="mappedProjects"></param>
        /// <returns>List of new bounded assemblies</returns>
        private List<TestAssembly> BindTestAssembliesWithBuilds(TestImportConfig importedConfig, IEnumerable<Project> mappedProjects)
        {
            var assemblies = new List<TestAssembly>();

            foreach (var project in importedConfig.Projects)
            {
                var mappedAssemblies = mapper.GetAssemblies(project.TestAssemblies).ToList();

                for (int index = 0; index < mappedAssemblies.Count(); index++)
                {
                    var mappedAssembly = mappedAssemblies[0];
                    var assemblyProject = mappedProjects.FirstOrDefault(proj => proj.Name == project.Name);
                    mappedAssembly.ParentObjId = assemblyProject.Id;
                    mappedAssembly.BuildId = assemblyProject.LatestBuildId;
                    mappedAssembly.Status = ObjectStatus.InQueue;

                    TestAssemblyRepository.Instance.Insert(mappedAssembly);
                }

                assemblies.AddRange(mappedAssemblies);
            }

            return assemblies;
        }

        /// <summary>
        /// Create new test suites. Bind each suite with corresponding build and assembly. Save suite in DB
        /// </summary>
        /// <param name="importedConfig"></param>
        /// <param name="builds"></param>
        /// <param name="assemblies"></param>
        /// <returns>Return updated test assemblies (bounded with suites)</returns>
        private List<TestAssembly> BindTestSuitesWithAssemblies(TestImportConfig importedConfig, List<Build> builds, List<TestAssembly> assemblies)
        {
            foreach (var projectConfig in importedConfig.Projects)
            {
                foreach (var testConfigAssembly in projectConfig.TestAssemblies)
                {
                    var mappedSuites = mapper.GetSuites(testConfigAssembly.TestSuites).ToList();
                    var currentAssembly = assemblies.Find(item => item.Name == testConfigAssembly.Name);

                    for (int index = 0; index < mappedSuites.Count(); index ++)
                    {
                        var suite = mappedSuites[index];

                        suite.BuildId = currentAssembly.BuildId;
                        suite.ParentObjId = currentAssembly.Id;
                        suite.Status = ObjectStatus.InQueue;

                        TestSuiteRepository.Instance.Insert(suite);
                    }

                    currentAssembly.TestSuites = mappedSuites.ToDictionary(item => item.Id, item => item);
                    TestAssemblyRepository.Instance.FlushChanges();
                }
            }

            return assemblies;
        }

        /// <summary>
        /// Create new test cases. Bind each test case with corresponding build, suite. Save test case in DB
        /// </summary>
        /// <param name="importedConfig"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        private void BindTestCasesWithSuites(TestImportConfig importedConfig, List<TestAssembly> assemblies)
        {
            // map cases
            // bind cases with suites
            // save cases in db

            foreach (var projectConfig in importedConfig.Projects)
            {
                foreach (var testConfigAssembly in projectConfig.TestAssemblies)
                {
                    foreach (var testSuiteConfig in testConfigAssembly.TestSuites)
                    {
                        var currentAssembly = assemblies.Find(item => item.Name == testConfigAssembly.Name);
                        var currentSuite = currentAssembly.TestSuites.ToList().Find(item => item.Value.Name == testSuiteConfig.Name);

                        var mappedCases = mapper.GetCases(testSuiteConfig.TestCases).ToList();

                        for (int index = 0; index < mappedCases.Count; index++)
                        {
                            var testCase = mappedCases[index];
                            testCase.Status = ObjectStatus.InQueue;
                            testCase.BuildId = currentAssembly.BuildId;
                            testCase.ParentObjId = currentSuite.Key;

                            TestCaseRepository.Instance.Insert(testCase);
                        }

                        currentSuite.Value.TestCases = mappedCases.ToDictionary(item => item.Id, item => item);
                        TestSuiteRepository.Instance.FlushChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Create new screenshots. Bind each screenshot with corresponding build, suite. Save test case in DB
        /// </summary>
        /// <param name="importedConfig"></param>
        /// <param name="assemblies"></param>
        private void BindScreenshotsWithTestCases(TestImportConfig importedConfig, List<TestAssembly> assemblies)
        {
            // map screenshots
            // bind screenshots with cases
            // save cases in db

            foreach (var projectConfig in importedConfig.Projects)
            {
                foreach (var testConfigAssembly in projectConfig.TestAssemblies)
                {
                    foreach (var testSuiteConfig in testConfigAssembly.TestSuites)
                    {
                        foreach (var testCaseConfig in testSuiteConfig.TestCases)
                        {
                            var currentAssembly = assemblies.Find(item => item.Name == testConfigAssembly.Name);
                            var currentSuite = currentAssembly.TestSuites.ToList().Find(item => item.Value.Name == testSuiteConfig.Name);
                            var currentCase = currentSuite.Value.TestCases.ToList().Find(item => item.Value.Name == testCaseConfig.Name);

                            var screenShots = testCaseConfig.ScreenShots;

                            for (int index = 0; index < screenShots.Count; index++)
                            {
                                var screenShot = screenShots[index];
                                screenShot.BuildId = currentCase.Value.BuildId;
                                screenShot.ParentObjId = currentCase.Key;
                                screenShot.Status = ObjectStatus.InQueue;

                                ScreenShotRepository.Instance.Insert(screenShot);
                            }

                            currentCase.Value.ScreenShots = screenShots.ToDictionary(item => item.Id, item => item);
                            TestCaseRepository.Instance.FlushChanges();
                        }
                    }
                }
            }
        }
    }
}