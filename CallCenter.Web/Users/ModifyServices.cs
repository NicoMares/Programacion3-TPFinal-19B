using CallCenter.Business.Repositories;
using System;

namespace CallCenter.Web.Users
{
    internal class ModifyService
    {
        private UserRepository repo;

        public ModifyService(UserRepository repo)

        {
            this.repo = repo;
        }

        internal bool Modify(string username, string fullName, string email, string pass, out Guid id, out string err)
        {
            throw new NotImplementedException();
        }

        internal bool Modify(string username, string fullName, string email, out Guid id, out string err)
        {
            throw new NotImplementedException();
        }
    }
}