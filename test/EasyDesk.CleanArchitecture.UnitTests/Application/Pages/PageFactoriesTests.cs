using EasyDesk.CleanArchitecture.Application.Pages;
using Shouldly;
using System.Collections.Generic;
using Xunit;
using static System.Linq.Enumerable;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Pages;

public class PageFactoriesTests
{
    [Theory]
    [MemberData(nameof(EmptyPageData))]
    public void GetPage_ShouldReturnAnEmptyPage_IfThePageIndexIsTooHigh(
        int index, int size, IEnumerable<int> items)
    {
        var pagination = new Pagination(index, size);
        items.GetPage(pagination).Items.ShouldBeEmpty();
    }

    public static IEnumerable<object[]> EmptyPageData()
    {
        var start = 1;
        var pageSize = 3;
        yield return new object[] { 0, pageSize, Empty<int>() };
        yield return new object[] { 2, pageSize, Empty<int>() };
        yield return new object[] { 1, pageSize, Range(start, pageSize) };
        yield return new object[] { 2, pageSize, Range(start, pageSize) };
        yield return new object[] { 3, pageSize, Range(start, pageSize * 3) };
        yield return new object[] { 4, pageSize, Range(start, pageSize * 3) };
    }

    [Theory]
    [MemberData(nameof(NonEmptyPageData))]
    public void GetPage_ShouldReturnANonEmpty_IfTheSequenceContainsEnoughElements(
        int index, int size, IEnumerable<int> items, IEnumerable<int> page)
    {
        var pagination = new Pagination(index, size);
        items.GetPage(pagination).Items.ShouldBe(page);
    }

    public static IEnumerable<object[]> NonEmptyPageData()
    {
        var start = 1;
        var pageSize = 3;
        yield return new object[] { 0, pageSize, Range(1, pageSize * 3), Range(start, pageSize) };
        yield return new object[] { 2, pageSize, Range(1, pageSize * 3), Range(start + pageSize * 2, pageSize) };
        yield return new object[] { 0, pageSize, Range(1, pageSize - 1), Range(start, pageSize - 1) };
        yield return new object[] { 1, pageSize, Range(1, pageSize * 2 - 1), Range(start + pageSize, pageSize - 1) };
    }
}
