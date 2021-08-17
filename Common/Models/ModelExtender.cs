using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Widgets.UserManuals.Domain;

namespace Nop.Plugin.Widgets.UserManuals.Models
{
    public static partial class ModelExtender
    {
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return AutoMapperConfiguration.Mapper.Map(source, destination);
        }

        public static UserManualModel ToModel(this UserManual entity)
        {
            return entity.MapTo<UserManual, UserManualModel>();
        }

        public static UserManual ToEntity(this UserManualModel model)
        {
            return model.MapTo<UserManualModel, UserManual>();
        }


        public static UserManualProductModel ToModel(this UserManualProduct entity)
        {
            return entity.MapTo<UserManualProduct, UserManualProductModel>();
        }

        public static UserManualProduct ToEntity(this UserManualProductModel model)
        {
            return model.MapTo<UserManualProductModel, UserManualProduct>();
        }

        public static UserManual ToEntity(this UserManualModel model, UserManual destination)
        {
            return model.MapTo(destination);
        }

        public static CategoryModel ToModel(this UserManualCategory entity)
        {
            return entity.MapTo<UserManualCategory, CategoryModel>();
        }

        public static UserManualCategory ToEntity(this CategoryModel model)
        {
            return model.MapTo<CategoryModel, UserManualCategory>();
        }

        public static UserManualCategory ToEntity(this CategoryModel model, UserManualCategory destination)
        {
            return model.MapTo(destination);
        }
    }
}
