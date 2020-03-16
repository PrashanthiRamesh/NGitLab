﻿using System;
using System.Collections.Generic;
using System.Linq;
using NGitLab.Models;

namespace NGitLab.Mock.Clients
{
    internal sealed class BranchClient : ClientBase, IBranchClient
    {
        private readonly int _projectId;

        public BranchClient(ClientContext context, int projectId)
            : base(context)
        {
            _projectId = projectId;
        }

        public Branch this[string name]
        {
            get
            {
                var project = GetProject(_projectId, ProjectPermission.View);
                return project.Repository.GetBranch(name).ToBranchClient(project);
            }
        }

        public IEnumerable<Branch> All
        {
            get
            {
                var project = GetProject(_projectId, ProjectPermission.View);
                return project.Repository.GetAllBranches().Select(branch => branch.ToBranchClient(project));
            }
        }

        public Branch Create(BranchCreate branch)
        {
            var project = GetProject(_projectId, ProjectPermission.Contribute);
            if (branch.Ref != null)
            {
                return project.Repository.CreateBranch(branch.Name, branch.Ref).ToBranchClient(project);
            }

            return project.Repository.CreateBranch(branch.Name).ToBranchClient(project);
        }

        public void Delete(string name)
        {
            var project = GetProject(_projectId, ProjectPermission.Contribute);
            project.Repository.RemoveBranch(name);
        }

        public Branch Protect(string name)
        {
            throw new NotImplementedException();
        }

        public Branch Unprotect(string name)
        {
            throw new NotImplementedException();
        }
    }
}
