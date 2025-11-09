using CallCenter.Business.Repositories;
using System;

namespace CallCenter.Web.Account
{
    internal class AssignRoleService
    {
        private UserRepository repo;

        public AssignRoleService(UserRepository repo)
        {
            this.repo = repo;
        }

        internal bool AssignRole(string username, string fullName, string email, string pass, out Guid id, out string err)
        {
            throw new NotImplementedException();
        }

        internal bool AssignRole(string username, string fullName, string email, out Guid id, out string err)
        {
            throw new NotImplementedException();
        }
    }
}