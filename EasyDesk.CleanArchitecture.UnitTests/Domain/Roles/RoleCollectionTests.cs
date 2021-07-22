using EasyDesk.CleanArchitecture.Domain.Roles;
using Shouldly;
using Xunit;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.Common.UnitTests.Domain.Roles
{
    public class RoleCollectionTests
    {
        private readonly Role _adminRole = Role.From("Admin");
        private readonly Role _supervisorRole = Role.From("Supervisor");
        private readonly Role _userRole = Role.From("User");

        private readonly RoleCollection _sut;

        public RoleCollectionTests()
        {
            _sut = RoleCollection.Create(
            _userRole,
            _supervisorRole);
        }

        [Fact]
        public void Add_ShouldAddTheGivenRole_IfItDoesNotExist()
        {
            var result = _sut.Add(_adminRole);

            result.ShouldBe(RoleCollection.Create(
                _userRole,
                _supervisorRole,
                _adminRole));
        }

        [Fact]
        public void Add_ShouldNotModifyTheCollection_IfTheRoleAlreadyExists()
        {
            var result = _sut.Add(_userRole);

            result.ShouldBe(_sut);
        }

        [Fact]
        public void Remove_ShouldRemoveTheGivenRole_IfTheRoleExists()
        {
            var result = _sut.Remove(_supervisorRole);

            result.ShouldBe(RoleCollection.Create(_userRole));
        }

        [Fact]
        public void Remove_ShouldNotModifyTheCollection_IfTheRoleDoesNotExist()
        {
            var result = _sut.Remove(_adminRole);

            result.ShouldBe(_sut);
        }

        [Fact]
        public void Contains_ShouldReturnTrue_IfTheRoleExists()
        {
            _sut.Contains(_userRole).ShouldBeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnFalse_IfTheRoleDoesNotExist()
        {
            _sut.Contains(_adminRole).ShouldBeFalse();
        }

        [Fact]
        public void EnumeratingTheCollection_ShouldReturnAllTheRolesInTheCollection()
        {
            _sut.ShouldBe(Items(_userRole, _supervisorRole), ignoreOrder: true);
        }
    }
}