using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using LibCanOpen;
using System;

namespace EDSEditorGUI2.Mapper
{
    public partial class ProtobufferViewModelMapper
    {
        public static ViewModels.Device MapFromProtobuffer(CanOpenDevice source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Timestamp, DateTime>().ConvertUsing(ts => ts.ToDateTime());
                cfg.CreateMap<CanOpen_FileInfo, ViewModels.FileInfo>()
                .ForMember(dest => dest.FileVersion, opt => opt.MapFrom(src => src.FileVersion))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreationTime, opt => opt.MapFrom(src => src.CreationTime))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.ModificationTime, opt => opt.MapFrom(src => src.ModificationTime))
                .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy));
                cfg.CreateMap<Google.Protobuf.Collections.MapField<string, OdObject>, ViewModels.ObjectDictionary>().ConvertUsing<ODConverter>();
                cfg.CreateMap<CanOpenDevice, ViewModels.Device>()
                .ForMember(dest => dest.FileInfo, opt => opt.MapFrom(src => src.FileInfo))
                .ForMember(dest => dest.DeviceInfo, opt => opt.MapFrom(src => src.DeviceInfo))
                .ForMember(dest => dest.DeviceCommissioning, opt => opt.MapFrom(src => src.DeviceCommissioning))
                .ForMember(dest => dest.Objects, opt => opt.MapFrom(src => src.Objects));
                cfg.CreateMap<CanOpen_DeviceInfo, ViewModels.DeviceInfo>();
                cfg.CreateMap<CanOpen_DeviceCommissioning, ViewModels.DeviceCommissioning>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var result = mapper.Map<ViewModels.Device>(source);
            return result;
        }
        public class ODConverter : ITypeConverter<Google.Protobuf.Collections.MapField<string, OdObject>, ViewModels.ObjectDictionary>,
            ITypeConverter< ViewModels.ObjectDictionary, Google.Protobuf.Collections.MapField<string, OdObject>>
        {
            public ViewModels.ObjectDictionary Convert(Google.Protobuf.Collections.MapField<string, OdObject> source, ViewModels.ObjectDictionary destination, ResolutionContext context)
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<OdObject, ViewModels.OdObject>();
                    cfg.CreateMap<OdSubObject, ViewModels.OdSubObject>();
                });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();

                destination = [];
                foreach (var item in source)
                {
                    destination.Add(item.Key, mapper.Map<ViewModels.OdObject>(item.Value));
                }
                return destination;
            }
            public Google.Protobuf.Collections.MapField<string, OdObject> Convert(ViewModels.ObjectDictionary source, Google.Protobuf.Collections.MapField<string, OdObject> destination, ResolutionContext context)
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<ViewModels.OdObject, OdObject>();
                    cfg.CreateMap<ViewModels.OdSubObject, OdSubObject>();
                });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();

                destination = [];
                foreach (var item in source)
                {
                    destination.Add(item.Key, mapper.Map<OdObject>(item.Value));
                }
                return destination;
            }
        }

        public static CanOpenDevice MapToProtobuffer(ViewModels.Device source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                //TODO
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var result = mapper.Map<CanOpenDevice>(source);
            return result;
        }
    }
}
