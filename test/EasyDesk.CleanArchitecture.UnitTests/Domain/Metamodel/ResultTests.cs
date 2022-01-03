using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Domain.Metamodel.Results.ResultImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel;

public class ResultTests
{
    private const int Value = 5;

    private record MyDomainError : DomainError;

    [Fact]
    public void ResultsShouldSupportShortCircuitEvaluationWithOperatorOr()
    {
        var shouldNotBeCalled = Substitute.For<Func<int>>();
        var test = Success(Value) || Success(shouldNotBeCalled());
        test.IsSuccess.ShouldBeTrue();
        test.ShouldBe(Value);
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public void ResultsShouldSupportShortCircuitEvaluationWithOperatorAnd()
    {
        var shouldNotBeCalled = Substitute.For<Func<int>>();
        var test = Failure<int>(new MyDomainError()) && Success(shouldNotBeCalled());
        test.IsFailure.ShouldBeTrue();
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()();
    }
}
