/*
    This file is part of libEDSsharp.

    libEDSsharp is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    libEDSsharp is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with libEDSsharp.  If not, see <http://www.gnu.org/licenses/>.
 
    Copyright(c) 2024 Lars E. Susaas
*/

using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using LibCanOpen;
using System;
using System.Globalization;
using System.Linq;

namespace libEDSsharp
{
    /// <summary>
    /// Conversion class to/from EDS to protobuffer
    /// </summary>
    public class MappingEDS
    {
        /// <summary>
        /// Converts from protobuffer to EDS
        /// </summary>
        /// <param name="source">protobuffer device</param>
        /// <returns>new EDS device containing data from protobuffer device</returns>
        public static EDSsharp MapFromProtobuffer(CanOpenDevice source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CanOpenDevice, EDSsharp>()
                .ForMember(dest => dest.Dirty, opt => opt.Ignore())
                .ForMember(dest => dest.xddfilename_1_1, opt => opt.Ignore())
                .ForMember(dest => dest.xddfilenameStripped, opt => opt.Ignore())
                .ForMember(dest => dest.edsfilename, opt => opt.Ignore())
                .ForMember(dest => dest.dcffilename, opt => opt.Ignore())
                .ForMember(dest => dest.ODfilename, opt => opt.Ignore())
                .ForMember(dest => dest.ODfileVersion, opt => opt.Ignore())
                .ForMember(dest => dest.mdfilename, opt => opt.Ignore())
                .ForMember(dest => dest.xmlfilename, opt => opt.Ignore())
                .ForMember(dest => dest.xddfilename_1_0, opt => opt.Ignore())
                .ForMember(dest => dest.xddTemplate, opt => opt.Ignore())
                .ForMember(dest => dest.dummy_ods, opt => opt.Ignore())
                .ForMember(dest => dest.CO_storageGroups, opt => opt.Ignore())
                .ForMember(dest => dest.md, opt => opt.Ignore())
                .ForMember(dest => dest.oo, opt => opt.Ignore())
                .ForMember(dest => dest.mo, opt => opt.Ignore())
                .ForMember(dest => dest.c, opt => opt.Ignore())
                .ForMember(dest => dest.du, opt => opt.Ignore())
                .ForMember(dest => dest.td, opt => opt.Ignore())
                .ForMember(dest => dest.sm, opt => opt.Ignore())
                .ForMember(dest => dest.cm, opt => opt.Ignore())
                .ForMember(dest => dest.modules, opt => opt.Ignore())
                .ForMember(dest => dest.NodeID, opt => opt.Ignore())
                .ForMember(dest => dest.projectFilename, opt => opt.MapFrom(src => src.DeviceInfo.ProductName))
                .ForMember(dest => dest.NodeID, opt => opt.MapFrom(src => src.DeviceCommissioning.NodeId))
                .ForMember(dest => dest.fi, opt => opt.MapFrom(src => src.FileInfo))
                .ForMember(dest => dest.di, opt => opt.MapFrom(src => src.DeviceInfo))
                .ForMember(dest => dest.dc, opt => opt.MapFrom(src => src.DeviceCommissioning))
                .ForMember(dest => dest.ods, opt => opt.MapFrom(src => src.Objects));
                cfg.CreateMap<CanOpen_FileInfo, FileInfo>()
                .ForMember(dest => dest.FileName, opt => opt.Ignore())
                .ForMember(dest => dest.LastEDS, opt => opt.Ignore())
                .ForMember(dest => dest.EDSVersionMajor, opt => opt.Ignore())
                .ForMember(dest => dest.EDSVersionMinor, opt => opt.Ignore())
                .ForMember(dest => dest.EDSVersion, opt => opt.Ignore())
                .ForMember(dest => dest.exportFolder, opt => opt.Ignore())
                .ForMember(dest => dest.FileRevision, opt => opt.MapFrom(src => (byte)src.FileVersion.ElementAtOrDefault(0)))
                .ForMember(dest => dest.CreationDateTime, opt => opt.MapFrom(src => src.CreationTime.ToDateTime()))
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationTime.ToDateTime().ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.CreationTime, opt => opt.MapFrom(src => src.CreationTime.ToDateTime().ToString("h:mmtt")))
                .ForMember(dest => dest.ModificationDateTime, opt => opt.MapFrom(src => src.ModificationTime.ToDateTime()))
                .ForMember(dest => dest.ModificationDate, opt => opt.MapFrom(src => src.ModificationTime.ToDateTime().ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.ModificationTime, opt => opt.MapFrom(src => src.ModificationTime.ToDateTime().ToString("h:mmtt")));
                cfg.CreateMap<CanOpen_DeviceInfo, DeviceInfo>()
                .ForMember(dest => dest.VendorNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ProductNumber, opt => opt.Ignore())
                .ForMember(dest => dest.RevisionNumber, opt => opt.Ignore())
                .ForMember(dest => dest.SimpleBootUpMaster, opt => opt.Ignore())
                .ForMember(dest => dest.SimpleBootUpSlave, opt => opt.Ignore())
                .ForMember(dest => dest.Granularity, opt => opt.Ignore())
                .ForMember(dest => dest.DynamicChannelsSupported, opt => opt.Ignore())
                .ForMember(dest => dest.CompactPDO, opt => opt.Ignore())
                .ForMember(dest => dest.GroupMessaging, opt => opt.Ignore())
                .ForMember(dest => dest.NrOfRXPDO, opt => opt.Ignore()) // TODO Calculate this
                .ForMember(dest => dest.NrOfTXPDO, opt => opt.Ignore()) // TODO Calculate this
                .ForMember(dest => dest.LSS_Supported, opt => opt.MapFrom(src => src.LssSlave))
                .ForMember(dest => dest.LSS_Master, opt => opt.MapFrom(src => src.LssMaster))
                .ForMember(dest => dest.NG_Slave, opt => opt.Ignore())
                .ForMember(dest => dest.NG_Master, opt => opt.Ignore())
                .ForMember(dest => dest.NrOfNG_MonitoredNodes, opt => opt.Ignore());
                cfg.CreateMap<CanOpen_DeviceCommissioning, DeviceCommissioning>()
                .ForMember(dest => dest.NetNumber, opt => opt.Ignore())
                .ForMember(dest => dest.NetworkName, opt => opt.Ignore())
                .ForMember(dest => dest.CANopenManager, opt => opt.Ignore())
                .ForMember(dest => dest.LSS_SerialNumber, opt => opt.Ignore());
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper.Map<EDSsharp>(source);
        }
        /// <summary>
        /// Converts from EDS to protobuffer
        /// </summary>
        /// <param name="source">EDS device</param>
        /// <returns>protobuffer device containing data from EDS</returns>
        public static CanOpenDevice MapToProtobuffer(EDSsharp source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<EDSsharp, CanOpenDevice>()
                .ForMember(dest => dest.FileInfo, opt => opt.MapFrom(src => src.fi))
                .ForMember(dest => dest.DeviceInfo, opt => opt.MapFrom(src => src.di))
                .ForMember(dest => dest.DeviceCommissioning, opt => opt.MapFrom(src => src.dc))
                .ForMember(dest => dest.Objects, opt => opt.MapFrom(src => src.ods));
                cfg.CreateMap<FileInfo, CanOpen_FileInfo>()
                .ForMember(dest => dest.CreationTime, opt => opt.MapFrom(new EDSDateAndTimeResolver("creation")))
                .ForMember(dest => dest.ModificationTime, opt => opt.MapFrom(new EDSDateAndTimeResolver("modification")));
                cfg.CreateMap<DeviceInfo, CanOpen_DeviceInfo>()
                .ForMember(dest => dest.BaudRate10, opt => opt.MapFrom(src => src.BaudRate_10))
                .ForMember(dest => dest.BaudRate20, opt => opt.MapFrom(src => src.BaudRate_20))
                .ForMember(dest => dest.BaudRate50, opt => opt.MapFrom(src => src.BaudRate_50))
                .ForMember(dest => dest.BaudRate125, opt => opt.MapFrom(src => src.BaudRate_125))
                .ForMember(dest => dest.BaudRate250, opt => opt.MapFrom(src => src.BaudRate_250))
                .ForMember(dest => dest.BaudRate500, opt => opt.MapFrom(src => src.BaudRate_500))
                .ForMember(dest => dest.BaudRate800, opt => opt.MapFrom(src => src.BaudRate_800))
                .ForMember(dest => dest.BaudRate1000, opt => opt.MapFrom(src => src.BaudRate_1000))
                .ForMember(dest => dest.BaudRateAuto, opt => opt.MapFrom(src => src.BaudRate_auto))
                .ForMember(dest => dest.LssSlave, opt => opt.MapFrom(src => src.LSS_Supported))
                .ForMember(dest => dest.LssMaster, opt => opt.MapFrom(src => src.LSS_Master));
                cfg.CreateMap<DeviceCommissioning, CanOpen_DeviceCommissioning>();
                cfg.CreateMap<ODentry, OdObject>()
                .ForMember(dest => dest.Disabled, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.StorageGroup, opt => opt.Ignore())
                .ForMember(dest => dest.FlagsPDO, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.parameter_name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.objecttype))
                .ForMember(dest => dest.CountLabel, opt => opt.MapFrom(src => src.Label));
                cfg.CreateMap<ObjectType, OdObject.Types.ObjectType>().ConvertUsing<ODTypeResolver>();
                cfg.CreateMap<ODentry, OdSubObject>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.parameter_name))
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.datatype))
                .ForMember(dest => dest.Sdo, opt => opt.MapFrom(src => src.AccessSDO()))
                .ForMember(dest => dest.Pdo, opt => opt.MapFrom(src => src.AccessPDO()))
                .ForMember(dest => dest.Srdo, opt => opt.Ignore())
                .ForMember(dest => dest.StringLengthMin, opt => opt.MapFrom(src => src.Lengthofstring));
            });

            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper.Map<CanOpenDevice>(source);
        }
    }


    /// <summary>
    /// Helper class to convert EDS date and time into datetime used in the protobuffer timestand (datetime)
    /// </summary>
    public class EDSDateAndTimeResolver : IValueResolver<FileInfo, CanOpen_FileInfo, Timestamp>
    {
        private readonly string _type;
        public EDSDateAndTimeResolver(string type)
        {
            _type = type;
        }
        /// <summary>
        /// Resolver to convert eds date and time into protobuffer timestamp (datetime)
        /// </summary>
        /// <param name="source">source EDS fileinfo object</param>
        /// <param name="destination">protobuffer fileinfo object</param>
        /// <param name="member">result object</param>
        /// <param name="context">resolve context</param>
        /// <returns>result </returns>
        public Timestamp Resolve(FileInfo source, CanOpen_FileInfo destination, Timestamp member, ResolutionContext context)
        {
            string strTime;
            string strDate;
            if (_type == "creation")
            {
                strDate = source.CreationDate;
                strTime = source.CreationTime;
            }
            else
            {
                strDate = source.ModificationDate;
                strTime = source.ModificationTime;
            }

            var time = new DateTime(0);
            var date = new DateTime(0);

            try
            {
                time = DateTime.ParseExact(strTime, "h:mmtt", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                if (e is FormatException)
                {
                    //Silently ignore
                }
            }

            try
            {
                date = DateTime.ParseExact(strDate, "MM-dd-yyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                if (e is FormatException)
                {
                    //Silently ignore
                }
            }

            var datetime = date.AddTicks(time.TimeOfDay.Ticks);
            return Timestamp.FromDateTime(datetime.ToUniversalTime());
        }
    }

    /// <summary>
    /// Helper class to convert Enum types
    /// </summary>
    /// Checkout AutoMapper.Extensions.EnumMapping when .net framework is gone
    public class ODTypeResolver : ITypeConverter<ObjectType, OdObject.Types.ObjectType>
    {
        /// <summary>
        /// Resolver to convert eds date and time into protobuffer timestamp (datetime)
        /// </summary>
        /// <param name="source">source EDS fileinfo object</param>
        /// <param name="destination">protobuffer fileinfo object</param>
        /// <param name="member">result object</param>
        /// <param name="context">resolve context</param>
        /// <returns>result </returns>
        public OdObject.Types.ObjectType Convert(ObjectType source, OdObject.Types.ObjectType destination, ResolutionContext context)
        {
            switch (source)
            {
                case ObjectType.VAR:
                    return OdObject.Types.ObjectType.Var;
                case ObjectType.ARRAY:
                    return OdObject.Types.ObjectType.Array;
                case ObjectType.RECORD:
                    return OdObject.Types.ObjectType.Record;
                case ObjectType.UNKNOWN:
                case ObjectType.NULL:
                case ObjectType.DOMAIN:
                case ObjectType.DEFTYPE:
                case ObjectType.DEFSTRUCT:
                default:
                    return OdObject.Types.ObjectType.Unspecified;
            }

        }
    }
}