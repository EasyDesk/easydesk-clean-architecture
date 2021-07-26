using EasyDesk.CleanArchitecture.Domain.Roles;
using Shouldly;
using System;
using Xunit;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Roles
{
    public class AbstractRoleTests
    {
        private const string _primaryRole = "PrimaryRole";
        private const string _secondaryRole = "SecondaryRole";

        private class TestRole : AbstractRole<TestRole>
        {
            public TestRole(RoleId roleId) : base(roleId)
            {

            }

            public static TestRole PrimaryRole { get; } = new(RoleId.From(_primaryRole));

            public static TestRole SecondaryRole { get; } = new(RoleId.From(_secondaryRole));
        }

        [Fact]
        public void ToString_ShouldReturnTheRoleIdAsAString()
        {
            TestRole.PrimaryRole.ToString().ShouldBe(_primaryRole);
        }

        [Fact]
        public void List_ShouldReturnAllDeclaredRoles()
        {
            TestRole.List().ShouldBe(Items(TestRole.PrimaryRole, TestRole.SecondaryRole), ignoreOrder: true);
        }

        [Fact]
        public void From_ShouldReturnTheCorrespondingRole_IfItExists()
        {
            TestRole.From(TestRole.PrimaryRole.ToString()).ShouldBe(TestRole.PrimaryRole);
        }

        [Fact]
        public void From_ShouldFail_IfTheRoleIdDoesNotExist()
        {
            Should.Throw<ArgumentException>(() => TestRole.From("IncorrectRole"));
        }

        [Fact]
        public void FromRoleId_ShouldReturnTheCorrespondingRole_IfItExists()
        {
            TestRole.From(TestRole.PrimaryRole.Id).ShouldBe(TestRole.PrimaryRole);
        }

        [Fact]
        public void FromRoleId_ShouldFail_IfTheRoleIdDoesNotExist()
        {
            Should.Throw<ArgumentException>(() => TestRole.From(RoleId.From("IncorrectRole")));
        }
    }
}
