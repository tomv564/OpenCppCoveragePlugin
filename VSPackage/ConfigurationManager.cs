// OpenCppCoverage is an open source code coverage for C++.
// Copyright (C) 2014 OpenCppCoverage
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCppCoverage.VSPackage
{
    class ConfigurationManager: IConfigurationManager
    {
        //---------------------------------------------------------------------
        public static readonly string ProjectNotMarkedAsBuildError
            = "The project {0} is marked as not build for the active solution configuration. "
                + "Please check your solution Configuration Manager.";

        //---------------------------------------------------------------------
        public DynamicVCConfiguration GetConfiguration(
            IEnumerable<SolutionContext> contexts,
            ExtendedProject project)
        {
            string error;
            var configuration = ComputeConfiguration(contexts, project, out error);
            
            if (configuration == null)
                throw new VSPackageException(error);

            return configuration;
        }

        //---------------------------------------------------------------------
        public DynamicVCConfiguration FindConfiguration(
            IEnumerable<SolutionContext> contexts,
            ExtendedProject project)
        {
            string error;
            var configuration = ComputeConfiguration(contexts, project, out error);
            return configuration;
        }

        //---------------------------------------------------------------------
        public string GetSolutionConfigurationName(SolutionConfiguration2 activeConfiguration)
        {
            return activeConfiguration.Name + '|' + activeConfiguration.PlatformName;

        }

        //---------------------------------------------------------------------
        DynamicVCConfiguration ComputeConfiguration(
            IEnumerable<SolutionContext> contexts,
            ExtendedProject project, 
            out string error)
        {
            error = null;
            var context = ComputeContext(contexts, project, ref error);

            if (context == null)
                return null;

            if (!context.ShouldBuild)
            {
                error = string.Format(ProjectNotMarkedAsBuildError, project.UniqueName);
                return null;
            }

            return ComputeConfiguration(project, context, ref error);
        }

        //---------------------------------------------------------------------
        static DynamicVCConfiguration ComputeConfiguration(
            ExtendedProject project, 
            SolutionContext context, 
            ref string error)
        {
            dynamic vcProject = project.project_.Object;
            dynamic vcConfig = vcProject.ActiveConfiguration;

            if (vcConfig == null)
            {
                var builder = new StringBuilder();

                builder.AppendLine(string.Format("Cannot find a configuration for the project {0}", project.UniqueName));
                builder.AppendLine(string.Format(" - Solution: configuration: {0} platform: {1}", context.ConfigurationName, context.PlatformName));
                foreach (var config in project.Configurations)
                    builder.AppendLine(string.Format(" - Project: configuration: {0} platform: {1}", config.ConfigurationName, config.PlatformName));
                error = builder.ToString();
                return null;
            }

            return new DynamicVCConfiguration(vcConfig);
        }

        //---------------------------------------------------------------------
        SolutionContext ComputeContext(
            IEnumerable<SolutionContext> contexts,
            ExtendedProject project, 
            ref string error)
        {
            var context = contexts.FirstOrDefault(c => c.ProjectName == project.UniqueName);

            if (context == null)
            {
                error = string.Format("Cannot find {0} in project contexts. "
                        + "Please check your solution Configuration Manager.",
                        project.UniqueName);
                return null;
            }

            return context;
        }
    }
}
