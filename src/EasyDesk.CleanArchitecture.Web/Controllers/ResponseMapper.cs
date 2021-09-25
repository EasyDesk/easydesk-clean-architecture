using AutoMapper;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Web.Controllers
{
    public delegate Task<IActionResult> RequestToBeMappedDelegate<T>(Func<Response<T>, ResponseDto> mapper);

    public class ResponseMapper<T>
    {
        private readonly RequestToBeMappedDelegate<T> _requestToBeMapped;
        private readonly IMapper _mapper;

        public ResponseMapper(RequestToBeMappedDelegate<T> requestToBeMapped, IMapper mapper)
        {
            _requestToBeMapped = requestToBeMapped;
            _mapper = mapper;
        }
        
        internal Task<IActionResult> Map(Func<T, IMapper, ResponseDto> responseFactory)
        {
            return _requestToBeMapped(res => res.Match(
                success: value => responseFactory(value, _mapper),
                failure: error => CreateErrorResponse(error)));
        }

        private ResponseDto CreateErrorResponse(Error error) => ResponseDto.FromError(_mapper.Map<ErrorDto>(error));
    }

    public static class ResultMapperExtensions
    {
        public static Task<IActionResult> MapTo<T, TDto>(this ResponseMapper<T> resultMapper)
        {
            return resultMapper.Map((value, mapper) => ResponseDto.FromData(mapper.Map<TDto>(value)));
        }

        public static Task<IActionResult> MapToMany<T, TDto>(this ResponseMapper<Page<T>> resultMapper)
        {
            return resultMapper.Map((page, mapper) =>
            {
                var meta = new PaginationResponseMetaDto(
                    page.Pagination.PageIndex,
                    page.Pagination.PageSize,
                    page.Count,
                    page.PageCount);
                return ResponseDto.FromData(mapper.Map<IEnumerable<TDto>>(page.Items), meta);
            });
        }

        public static Task<IActionResult> MapEmpty(this ResponseMapper<Nothing> resultMapper)
        {
            return resultMapper.Map((_, _) => ResponseDto.Empty());
        }
    }
}