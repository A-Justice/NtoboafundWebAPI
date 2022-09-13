using System;
using AutoMapper;
using NtoboaFund.Data.DTOs;
using NtoboaFund.Data.Models;

namespace NtoboaFund.Helpers.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CrowdFundMappings();
            DonationMappings();
        }

        void CrowdFundMappings()
        {
            CreateMap<CrowdFund, CrowdFundForReturnDTO>()
            .ForMember(d => d.Username, opt =>
            {
                opt.MapFrom(s => s.User.FirstName + " " + s.User.LastName);
            }).ForMember(d => d.CrowdfundTypeName, opt =>
            {
                opt.MapFrom(s => s.CrowdFundType.Name);
            }).ForMember(d => d.MainImageUrl, opt =>
            {
                opt.MapFrom(s => s.MainImageUrl??"");
            }).ForMember(d => d.SecondImageUrl, opt =>
            {
                opt.MapFrom(s => s.SecondImageUrl??"");
            }).ForMember(d => d.ThirdImageUrl, opt =>
            {
                opt.MapFrom(s => s.ThirdImageUrl??"");
            }).ForMember(d => d.videoUrl, opt =>
            {
                opt.MapFrom(s => s.videoUrl??"");
            });
        }

        void DonationMappings()
        {
            CreateMap<Donation, DonationForReturnDTO>()
                .ForMember(d => d.Username, opt =>
                {
                    opt.MapFrom(c => c.User.FirstName + " " + c.User.LastName);
                });
        }
    }
}
