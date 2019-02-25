﻿using System;
using System.Linq;
using NGitLab.Models;
using NUnit.Framework;

namespace NGitLab.Tests
{
    public class RunnerTests
    {
        [Test]
        public void Test()
        {
            var runnerToEnable = GetDefaultRunner();
            var runners = Initialize.GitLabClient.Runners;

#pragma warning disable 618 // Obsolete
            var jobsOld = runners.GetJobs(runnerToEnable.Id, JobScope.All).Take(1);
#pragma warning restore 618
            var jobs = runners.GetJobs(runnerToEnable.Id).Take(1);

            Assert.That(jobsOld, Is.Not.Empty);
            Assert.That(jobs, Is.Not.Empty);
        }

        [Test]
        public void Test_can_enable_and_disable_a_runner_on_a_project()
        {
            var projectId = Initialize.UnitTestProject.Id;

            var runnerToEnable = GetDefaultRunner();
            var runners = Initialize.GitLabClient.Runners;

            if (IsEnabled(runnerToEnable, projectId))
            {
                runners.DisableRunner(projectId, new RunnerId(runnerToEnable.Id));
            }

            runners.EnableRunner(projectId, new RunnerId(runnerToEnable.Id));
            Assert.IsTrue(IsEnabled(runnerToEnable, projectId));

            runners.DisableRunner(projectId, new RunnerId(runnerToEnable.Id));
            Assert.IsFalse(IsEnabled(runnerToEnable, projectId));
        }

        [Test]
        public void Test_can_find_a_runner_on_a_project()
        {
            var projectId = Initialize.UnitTestProject.Id;
            var runnerToEnable = GetDefaultRunner();
            var runners = Initialize.GitLabClient.Runners;
            runners.EnableRunner(projectId, new RunnerId(runnerToEnable.Id));

            var result = Initialize.GitLabClient.Runners.OfProject(projectId).ToList();
            Assert.IsTrue(result[0].Id == runnerToEnable.Id);

            runners.DisableRunner(projectId, new RunnerId(runnerToEnable.Id));
            result = Initialize.GitLabClient.Runners.OfProject(projectId).ToList();
            Assert.IsEmpty(result);
        }

        [Test]
        public void Test_some_runner_fields_are_not_null()
        {
            var projectId = Initialize.UnitTestProject.Id;
            var runnerToEnable = GetDefaultRunner();
            var runners = Initialize.GitLabClient.Runners;
            runners.EnableRunner(projectId, new RunnerId(runnerToEnable.Id));

            var result = Initialize.GitLabClient.Runners.OfProject(projectId).ToList();
            var runnerId = result[0].Id;
            var runnerDetails = runners[runnerId];

            Assert.IsNotNull(runnerDetails.Id);
            Assert.IsNotNull(runnerDetails.Active);
            Assert.IsNotNull(runnerDetails.Locked);
            Assert.IsNotNull(runnerDetails.RunUntagged);
            Assert.IsNotNull(runnerDetails.IpAddress);

            runners.DisableRunner(projectId, new RunnerId(runnerToEnable.Id));
            result = Initialize.GitLabClient.Runners.OfProject(projectId).ToList();
            Assert.IsEmpty(result);
        }

        [Test]
        public void Test_Runner_Can_Be_Locked_And_Unlocked()
        {
            var projectId = Initialize.UnitTestProject.Id;
            var runnerToEnable = GetDefaultRunner();
            var runners = Initialize.GitLabClient.Runners;
            runners.EnableRunner(projectId, new RunnerId(runnerToEnable.Id));

            var result = Initialize.GitLabClient.Runners.OfProject(projectId).ToList();
            var runnerId = result[0].Id;
            var runnerDetails = runners[runnerId];

            // assert runner is not locked
            Assert.IsNotNull(runnerDetails.Id);
            Assert.IsFalse(runnerDetails.Locked);

            // lock runner
            var lockingRunner = new RunnerUpdate
            {
                Locked = true,
                Description = runnerDetails.Description,
                Active = runnerDetails.Active,
                TagList = runnerDetails.TagList
            };

            var updatedRunner = runners.Update(runnerToEnable.Id, lockingRunner);

            // assert runner is locked
            Assert.IsNotNull(updatedRunner.Id);
            Assert.IsFalse(updatedRunner.Locked);

            // unlock runner
            lockingRunner = new RunnerUpdate()
            {
                Locked = false,
                Description = runnerDetails.Description,
                Active = runnerDetails.Active,
                TagList = runnerDetails.TagList
            };

            runners.Update(runnerToEnable.Id, lockingRunner);
            runners.DisableRunner(projectId, new RunnerId(runnerToEnable.Id));
        }

        private static bool IsEnabled(Runner runner, int projectId)
        {
            return Initialize.GitLabClient.Runners[runner.Id].Projects.Any(x => x.Id == projectId);
        }

        public static Runner GetDefaultRunner()
        {
            var allRunners = Initialize.GitLabClient.Runners.Accessible.ToArray();
            var runner = Array.Find(allRunners, x => x.Active && string.Equals(x.Description, "ToolSquare_Test", StringComparison.Ordinal));

            if (runner == null)
            {
                Assert.Inconclusive("Will not be able to test the builds as no runner is setup for this project");
            }

            return runner;
        }
    }
}
