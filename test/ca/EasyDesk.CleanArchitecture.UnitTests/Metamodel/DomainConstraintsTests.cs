using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Collections;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Metamodel;

public class DomainConstraintsTests
{
    public record SomeError(int I) : DomainError;

    [Fact]
    public void ShouldNotThrow_IfConditionsArentMet_UsingIf()
    {
        DomainConstraints.Check()
            .If(false, () => new SomeError(1))
            .ThrowException();
    }

    [Fact]
    public void ShouldNotThrow_IfConditionsArentMet_UsingIfNot()
    {
        DomainConstraints.Check()
            .IfNot(true, () => new SomeError(1))
            .ThrowException();
    }

    [Fact]
    public void ShouldNotThrow_IfConditionsArentMet_UsingElseIf()
    {
        DomainConstraints.Check()
            .ElseIf(false, () => new SomeError(1))
            .ThrowException();
    }

    [Fact]
    public void ShouldNotThrow_IfConditionsArentMet_UsingElseIfNot()
    {
        DomainConstraints.Check()
            .ElseIfNot(true, () => new SomeError(1))
            .ThrowException();
    }

    [Fact]
    public void ShouldThrow_IfConditionsAreMet_UsingIf()
    {
        Should.Throw<DomainConstraintException>(() =>
        {
            DomainConstraints.Check()
                .If(true, () => new SomeError(1))
                .ThrowException();
        }).DomainErrors.ShouldBe(EnumerableUtils.Items(new SomeError(1)));
    }

    [Fact]
    public void ShouldThrow_IfConditionsAreMet_UsingElseIf()
    {
        Should.Throw<DomainConstraintException>(() =>
        {
            DomainConstraints.Check()
                .ElseIf(true, () => new SomeError(1))
                .ThrowException();
        }).DomainErrors.ShouldBe(EnumerableUtils.Items(new SomeError(1)));
    }

    [Fact]
    public void ShouldThrow_IfConditionsAreMet_UsingIfNot()
    {
        Should.Throw<DomainConstraintException>(() =>
        {
            DomainConstraints.Check()
                .IfNot(false, () => new SomeError(1))
                .ThrowException();
        }).DomainErrors.ShouldBe(EnumerableUtils.Items(new SomeError(1)));
    }

    [Fact]
    public void ShouldThrow_IfConditionsAreMet_UsingElseIfNot()
    {
        Should.Throw<DomainConstraintException>(() =>
        {
            DomainConstraints.Check()
                .ElseIfNot(false, () => new SomeError(1))
                .ThrowException();
        }).DomainErrors.ShouldBe(EnumerableUtils.Items(new SomeError(1)));
    }

    [Fact]
    public void ShouldThrowMultiples_WhenNotElse()
    {
        Should.Throw<DomainConstraintException>(() =>
        {
            DomainConstraints.Check()
                .If(true, () => new SomeError(1))
                .IfNot(false, () => new SomeError(2))
                .ThrowException();
        }).DomainErrors.ShouldBe(EnumerableUtils.Items(new SomeError(1), new SomeError(2)));
    }

    [Fact]
    public void ShouldNotThrowMultiples_UsingElse()
    {
        Should.Throw<DomainConstraintException>(() =>
        {
            DomainConstraints.Check()
                .If(true, () => new SomeError(1))
                .ElseIfNot(false, () => new SomeError(2))
                .ElseIf(true, () => new SomeError(3))
                .ThrowException();
        }).DomainErrors.ShouldBe(EnumerableUtils.Items(new SomeError(1)));
    }

    [Fact]
    public void ShouldNotThrowMultiples_UsingElseMultipleTimes()
    {
        Should.Throw<DomainConstraintException>(() =>
        {
            DomainConstraints.Check()
                .If(false, () => new SomeError(1))
                .ElseIfNot(false, () => new SomeError(2))
                .ElseIf(true, () => new SomeError(3))
                .ThrowException();
        }).DomainErrors.ShouldBe(EnumerableUtils.Items(new SomeError(2)));
    }

    [Fact]
    public void ShouldThrowMultiples_WhenElsesAreMixed()
    {
        Should.Throw<DomainConstraintException>(() =>
        {
            DomainConstraints.Check()
                .If(false, () => new SomeError(1))
                .ElseIfNot(false, () => new SomeError(2))
                .IfNot(false, () => new SomeError(3))
                .ElseIf(true, () => new SomeError(4))
                .ThrowException();
        }).DomainErrors.ShouldBe(EnumerableUtils.Items(new SomeError(2), new SomeError(3)));
    }
}
