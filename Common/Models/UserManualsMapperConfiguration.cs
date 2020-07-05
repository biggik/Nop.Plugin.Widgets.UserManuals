using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Widgets.UserManuals.Domain;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    /// <summary>
    /// AutoMapper configuration for User Manuals models
    /// </summary>
    public class UserManualsMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public UserManualsMapperConfiguration()
        {
            CreateMap<UserManualModel, UserManual>();
            CreateMap<UserManual, UserManualModel>()
                .ForMember(um => um.Id, mo => mo.MapFrom(m => m.Id))
                .ForMember(um => um.ManufacturerId, mo => mo.MapFrom(m => m.ManufacturerId))
                .ForMember(um => um.CategoryId, mo => mo.MapFrom(m => m.CategoryId))
                .ForMember(um => um.Description, mo => mo.MapFrom(m => m.Description))
                .ForMember(um => um.DocumentId, mo => mo.MapFrom(m => m.DocumentId))
                .ForMember(um => um.OnlineLink, mo => mo.MapFrom(m => m.OnlineLink))
                .ForMember(um => um.Published, mo => mo.MapFrom(m => m.Published))
                .ForMember(um => um.DisplayOrder, mo => mo.MapFrom(m => m.DisplayOrder))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<UserManualProductModel, UserManualProduct>();
            CreateMap<UserManualProduct, UserManualProductModel>()
                .ForMember(um => um.Id, mo => mo.MapFrom(m => m.Id))
                .ForMember(um => um.UserManualId, mo => mo.MapFrom(m => m.UserManualId))
                .ForMember(um => um.ProductId, mo => mo.MapFrom(m => m.ProductId))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<CategoryModel, UserManualCategory>();
            CreateMap<UserManualCategory, CategoryModel>()
               .ForMember(cm => cm.Id, mo => mo.MapFrom(c => c.Id))
               .ForMember(cm => cm.Name, mo => mo.MapFrom(c => c.Name))
                .ForMember(cm => cm.Published, mo => mo.MapFrom(c => c.Published))
                .ForMember(cm => cm.DisplayOrder, mo => mo.MapFrom(c => c.DisplayOrder))
                .ForAllOtherMembers(x => x.Ignore());
            
        }

        /// <summary>
        /// Order of this mapper implumentation
        /// </summary>
        public int Order => 0;
    }
}