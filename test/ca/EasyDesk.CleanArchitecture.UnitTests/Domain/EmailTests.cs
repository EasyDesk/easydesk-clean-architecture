using EasyDesk.CleanArchitecture.Domain.Model;
using FluentValidation;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain;

public static class EmailTests
{
    public static IEnumerable<object[]> ValidEmails()
    {
        yield return ["a@a.it",];
        yield return ["A@a.it",];
        yield return ["a@A.it",];
        yield return [new string('x', Email.MaxLength - 5) + "@a.it",];
        yield return ["a@aaaaaaaaaaaaaaaaaaaaaaaaaaaa.it",];
        yield return ["a@a.com",];
        yield return ["a@a.org",];
        yield return ["a@a.b.c.d.org",];
        yield return ["a.213@a.com",];
        yield return ["a.213+xyz@a.com",];
        yield return ["aAaaAAee_asdf-ee.213+xyz@a.com",];
    }

    [Theory]
    [MemberData(nameof(ValidEmails))]
    public static void ShouldCreateNewEmails_WithValidInput(string address)
    {
        var sut = new Email(address);

        sut.Value.ShouldBe(address);
    }

    public static IEnumerable<object[]> InvalidEmails()
    {
        yield return [string.Empty,];
        yield return ["@",];
        yield return ["@.",];
        yield return ["@.it",];
        yield return ["@a.it",];
        yield return ["a@",];
        yield return ["a@.",];
        yield return ["a@.it",];
        yield return ["a@a._",];
        yield return ["a@a.b.c..org",];
        yield return [new string('x', Email.MaxLength - 4) + "@a.it",];
    }

    [Theory]
    [MemberData(nameof(InvalidEmails))]
    public static void ShouldThrowException_WhileCreatingNewEmail_WithInvalidInput(string address)
    {
        Should.Throw<ValidationException>(() => new Email(address));
    }
}
