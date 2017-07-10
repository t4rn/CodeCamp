using AutoMapper;
using MyCodeCamp.Data.Entities;

namespace CodeCamp.Api.Models
{
    public class CampMappingProfile : Profile
    {
        public CampMappingProfile()
        {
            CreateMap<Camp, CampModel>()
                .ForMember(c => c.StartDate, options => options.MapFrom(x => x.EventDate))
                .ForMember(c => c.EndDate, options => options.ResolveUsing(x => x.EventDate.AddDays(x.Length - 1)))
                .ForMember(c => c.Url, options => options.ResolveUsing<CampUrlResolver>())
                .ReverseMap()
                .ForMember(m => m.EventDate, options => options.MapFrom(model => model.StartDate))
                .ForMember(m => m.Length, options => options.ResolveUsing(model => (model.EndDate - model.StartDate).Days + 1))
                .ForMember(m => m.Location, options => options.ResolveUsing(c => new Location()
                {
                    Address1 = c.LocationAddress1,
                    Address2 = c.LocationAddress2,
                    Address3 = c.LocationAddress3,
                    CityTown = c.LocationCityTown,
                    StateProvince = c.LocationStateProvince,
                    PostalCode = c.LocationPostalCode,
                    Country = c.LocationCountry
                }))
                ;

            CreateMap<Speaker, SpeakerModel>()
                .ForMember(c => c.Url, options => options.ResolveUsing<SpeakerUrlResolver>())
                .ReverseMap()
                ;

            CreateMap<Speaker, Speaker2Model>()
                .IncludeBase<Speaker, SpeakerModel>()
                .ForMember(c => c.BadgeName, options => options.ResolveUsing(s => $"{s.Name} (@{s.TwitterName})"));

            CreateMap<Talk, TalkModel>()
                .ForMember(t => t.Url, options => options.ResolveUsing<TalkUrlResolver>())
                .ReverseMap();
        }
    }
}
