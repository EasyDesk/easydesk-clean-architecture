using EasyDesk.CleanArchitecture.Domain.Model.Roles;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.UserInfo
{
    public interface IUserInfo
    {
        bool IsLoggedIn { get; }

        Guid UserId { get; }

        IEnumerable<Role> Roles { get; }
    }
}
