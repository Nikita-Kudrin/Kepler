﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Kepler.Common.Models;
using Kepler.Core;
using Kepler.Models;

namespace Kepler.Service.Config
{
    public class ConfigMapper
    {
        public Project GetProject(TestImportConfig.ProjectConfig projectConfig)
        {
            Mapper.Reset();

            Mapper.Configuration.AllowNullCollections = true;
            Mapper.Configuration.AllowNullDestinationValues = true;

            Mapper.CreateMap<TestImportConfig.ProjectConfig, Project>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Branches, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            Mapper.AssertConfigurationIsValid();

            return Mapper.Map<TestImportConfig.ProjectConfig, Project>(projectConfig);
        }

        public IEnumerable<Project> GetProjects(IEnumerable<TestImportConfig.ProjectConfig> projectsConfig)
        {
            return projectsConfig.Select(config => GetProject(config));
        }

        public Branch GetBranch(TestImportConfig.BranchConfig branchConfig)
        {
            Mapper.Reset();

            Mapper.Configuration.AllowNullCollections = true;
            Mapper.Configuration.AllowNullDestinationValues = true;

            Mapper.CreateMap<TestImportConfig.BranchConfig, Branch>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.BaseLine, opt => opt.Ignore())
                .ForMember(dest => dest.Builds, opt => opt.Ignore())
                .ForMember(dest => dest.IsMainBranch, opt => opt.Ignore())
                .ForMember(dest => dest.LatestBuildId, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            Mapper.AssertConfigurationIsValid();

            return Mapper.Map<TestImportConfig.BranchConfig, Branch>(branchConfig);
        }

        public IEnumerable<Branch> GetBranches(IEnumerable<TestImportConfig.BranchConfig> branchesConfig)
        {
            return branchesConfig.Select(config => GetBranch(config));
        }

        public TestAssembly GetAssembly(TestImportConfig.TestAssemblyConfig assemblyConfig)
        {
            Mapper.Reset();

            Mapper.Configuration.AllowNullCollections = true;
            Mapper.Configuration.AllowNullDestinationValues = true;

            Mapper.CreateMap<TestImportConfig.TestAssemblyConfig, TestAssembly>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TestSuites, opt => opt.Ignore())
                .ForMember(dest => dest.BuildId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ParentObjId, opt => opt.Ignore());


            Mapper.AssertConfigurationIsValid();

            return Mapper.Map<TestImportConfig.TestAssemblyConfig, TestAssembly>(assemblyConfig);
        }

        public IEnumerable<TestAssembly> GetAssemblies(
            IEnumerable<TestImportConfig.TestAssemblyConfig> testAssemblyConfigs)
        {
            return testAssemblyConfigs.Select(config => GetAssembly(config));
        }

        public TestSuite GetSuite(TestImportConfig.TestSuiteConfig suiteConfig)
        {
            Mapper.Reset();

            Mapper.Configuration.AllowNullCollections = true;
            Mapper.Configuration.AllowNullDestinationValues = true;

            Mapper.CreateMap<TestImportConfig.TestSuiteConfig, TestSuite>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TestCases, opt => opt.Ignore())
                .ForMember(dest => dest.BuildId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ParentObjId, opt => opt.Ignore());

            Mapper.AssertConfigurationIsValid();

            return Mapper.Map<TestImportConfig.TestSuiteConfig, TestSuite>(suiteConfig);
        }

        public IEnumerable<TestSuite> GetSuites(IEnumerable<TestImportConfig.TestSuiteConfig> suiteConfigs)
        {
            return suiteConfigs.Select(config => GetSuite(config));
        }

        public TestCase GetCase(TestImportConfig.TestCaseConfig caseConfig)
        {
            Mapper.Reset();

            Mapper.Configuration.AllowNullCollections = true;
            Mapper.Configuration.AllowNullDestinationValues = true;

            Mapper.CreateMap<TestImportConfig.TestCaseConfig, TestCase>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ScreenShots, opt => opt.Ignore())
                .ForMember(dest => dest.BuildId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ParentObjId, opt => opt.Ignore());

            Mapper.AssertConfigurationIsValid();

            return Mapper.Map<TestImportConfig.TestCaseConfig, TestCase>(caseConfig);
        }

        public IEnumerable<TestCase> GetCases(IEnumerable<TestImportConfig.TestCaseConfig> testCaseConfigs)
        {
            return testCaseConfigs.Select(config => GetCase(config));
        }
    }
}