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
using System.Collections.Generic;
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
                // workaround for https://github.com/AutoMapper/AutoMapper/issues/2959
                // Cant update untill after .net framwork is gone
                cfg.ShouldMapMethod = (m => m.Name == "ARandomStringThatDoesNotMatchAnyFunctionName");
                cfg.CreateMap<string, UInt16>().ConvertUsing<ODStringToShortTypeResolver>();
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
                .ForMember(dest => dest.BaudRate_10, opt => opt.MapFrom(src => src.BaudRate10))
                .ForMember(dest => dest.BaudRate_20, opt => opt.MapFrom(src => src.BaudRate20))
                .ForMember(dest => dest.BaudRate_50, opt => opt.MapFrom(src => src.BaudRate50))
                .ForMember(dest => dest.BaudRate_125, opt => opt.MapFrom(src => src.BaudRate125))
                .ForMember(dest => dest.BaudRate_250, opt => opt.MapFrom(src => src.BaudRate250))
                .ForMember(dest => dest.BaudRate_500, opt => opt.MapFrom(src => src.BaudRate500))
                .ForMember(dest => dest.BaudRate_800, opt => opt.MapFrom(src => src.BaudRate800))
                .ForMember(dest => dest.BaudRate_1000, opt => opt.MapFrom(src => src.BaudRate1000))
                .ForMember(dest => dest.BaudRate_auto, opt => opt.MapFrom(src => src.BaudRateAuto))
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
                cfg.CreateMap<OdObject, CustomProperties>()
                .ForMember(dest => dest.CO_accessSRDO, opt => opt.Ignore())
                .ForMember(dest => dest.CO_stringLengthMin, opt => opt.Ignore())
                .ForMember(dest => dest.CO_disabled, opt => opt.MapFrom(src => src.Disabled))
                .ForMember(dest => dest.CO_countLabel, opt => opt.MapFrom(src => src.CountLabel))
                .ForMember(dest => dest.CO_storageGroup, opt => opt.MapFrom(src => src.StorageGroup))
                .ForMember(dest => dest.CO_flagsPDO, opt => opt.MapFrom(src => src.FlagsPDO));
                cfg.CreateMap<OdObject.Types.ObjectType, ObjectType>().ConvertUsing<ODTypeResolver>();
                cfg.CreateMap<OdObject, ODentry>()
                .ForMember(dest => dest.Index, opt => opt.Ignore())
                .ForMember(dest => dest.parameter_name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.denotation, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.datatype, opt => opt.Ignore())
                .ForMember(dest => dest.accesstype, opt => opt.Ignore())
                .ForMember(dest => dest.defaultvalue, opt => opt.Ignore())
                .ForMember(dest => dest.LowLimit, opt => opt.Ignore())
                .ForMember(dest => dest.HighLimit, opt => opt.Ignore())
                .ForMember(dest => dest.actualvalue, opt => opt.Ignore())
                .ForMember(dest => dest.ObjFlags, opt => opt.Ignore())
                .ForMember(dest => dest.CompactSubObj, opt => opt.Ignore())
                .ForMember(dest => dest.count, opt => opt.Ignore())
                .ForMember(dest => dest.ObjExtend, opt => opt.Ignore())
                .ForMember(dest => dest.PDOtype, opt => opt.Ignore())
                .ForMember(dest => dest.Label, opt => opt.Ignore())
                .ForMember(dest => dest.parent, opt => opt.Ignore())
                .ForMember(dest => dest.prop, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.uniqueID, opt => opt.Ignore());
                cfg.CreateMap<OdSubObject, EDSsharp.AccessType>().ConvertUsing<ODAccessTypeResolver>();
                cfg.CreateMap<OdSubObject, ODentry>()
                .ForMember(dest => dest.parameter_name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Index, opt => opt.Ignore())
                .ForMember(dest => dest.denotation, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.accesstype, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.ObjFlags, opt => opt.Ignore())
                .ForMember(dest => dest.CompactSubObj, opt => opt.Ignore())
                .ForMember(dest => dest.count, opt => opt.Ignore())
                .ForMember(dest => dest.ObjExtend, opt => opt.Ignore())
                .ForMember(dest => dest.PDOtype, opt => opt.Ignore())
                .ForMember(dest => dest.Label, opt => opt.Ignore())
                .ForMember(dest => dest.parent, opt => opt.Ignore())
                .ForMember(dest => dest.prop, opt => opt.Ignore())
                .ForPath(dest => dest.prop.CO_accessSRDO, opt => opt.MapFrom(src => src.Srdo))
                .ForPath(dest => dest.prop.CO_stringLengthMin, opt => opt.MapFrom(src => src.StringLengthMin))
                .ForMember(dest => dest.uniqueID, opt => opt.Ignore())
                .ForMember(dest => dest.objecttype, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.subobjects, opt => opt.Ignore());
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var result = mapper.Map<EDSsharp>(source);

            //Post processing, add index / subindex
            foreach (KeyValuePair<ushort, ODentry> obj in result.ods)
            {
                obj.Value.Index = obj.Key;
                foreach (KeyValuePair<ushort, ODentry> subObj in obj.Value.subobjects)
                {
                    subObj.Value.parent = obj.Value;
                }
            }
            return result;
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
                // workaround for https://github.com/AutoMapper/AutoMapper/issues/2959
                // Cant update untill after .net framwork is gone
                cfg.ShouldMapMethod = (m => m.Name == "ARandomStringThatDoesNotMatchAnyFunctionName");
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
                .ForMember(dest => dest.Disabled, opt => opt.MapFrom(src => src.prop.CO_disabled))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.denotation))
                .ForMember(dest => dest.StorageGroup, opt => opt.MapFrom(src => src.prop.CO_storageGroup))
                .ForMember(dest => dest.FlagsPDO, opt => opt.MapFrom(src => src.prop.CO_flagsPDO))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.parameter_name))
                .ForMember(dest => dest.ObjectType, opt => opt.MapFrom(src => src.objecttype))
                .ForMember(dest => dest.CountLabel, opt => opt.MapFrom(src => src.prop.CO_countLabel));
                cfg.CreateMap<ObjectType, OdObject.Types.ObjectType>().ConvertUsing<ODTypeResolver>();
                cfg.CreateMap<EDSsharp.AccessType, OdSubObject.Types.AccessSDO>().ConvertUsing<ODAccessTypeResolver>();
                cfg.CreateMap<EDSsharp.AccessType, OdSubObject.Types.AccessPDO>().ConvertUsing<ODAccessTypeResolver>();
                cfg.CreateMap<ODentry, OdSubObject>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.parameter_name))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.denotation))
                .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.datatype))
                .ForMember(dest => dest.Sdo, opt => opt.MapFrom(src => src.accesstype))
                .ForMember(dest => dest.Pdo, opt => opt.MapFrom(src => src.accesstype))
                .ForMember(dest => dest.Srdo, opt => opt.MapFrom(src => src.prop.CO_accessSRDO))
                .ForMember(dest => dest.StringLengthMin, opt => opt.MapFrom(src => src.prop.CO_stringLengthMin));
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
    /// Helper class to convert object type enum
    /// </summary>
    /// Checkout AutoMapper.Extensions.EnumMapping when .net framework is gone
    public class ODTypeResolver : ITypeConverter<ObjectType, OdObject.Types.ObjectType>, ITypeConverter<OdObject.Types.ObjectType, ObjectType>
    {
        /// <summary>
        /// Resolver to convert object types
        /// </summary>
        /// <param name="source">EDS object type object</param>
        /// <param name="destination">protobuffer object type</param>
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
        /// <summary>
        /// Resolver to convert object types
        /// </summary>
        /// <param name="source">EDS object type object</param>
        /// <param name="destination">protobuffer object type</param>
        /// <param name="member">result object</param>
        /// <param name="context">resolve context</param>
        /// <returns>result </returns>
        public ObjectType Convert(OdObject.Types.ObjectType source, ObjectType destination, ResolutionContext context)
        {
            switch (source)
            {
                case OdObject.Types.ObjectType.Unspecified:
                    return ObjectType.UNKNOWN;
                case OdObject.Types.ObjectType.Var:
                    return ObjectType.VAR;
                case OdObject.Types.ObjectType.Array:
                    return ObjectType.ARRAY;
                case OdObject.Types.ObjectType.Record:
                    return ObjectType.RECORD;
                default:
                    return ObjectType.UNKNOWN;
            }
        }
    }
    /// <summary>
    /// Helper class to convert Access types
    /// </summary>
    /// Checkout AutoMapper.Extensions.EnumMapping when .net framework is gone
    public class ODAccessTypeResolver : ITypeConverter<EDSsharp.AccessType, OdSubObject.Types.AccessSDO>,
                                           ITypeConverter<EDSsharp.AccessType, OdSubObject.Types.AccessPDO>,
                                           ITypeConverter<OdSubObject, EDSsharp.AccessType>
    {
        /// <summary>
        /// Resolver to convert eds access into SDO access type
        /// </summary>
        /// <param name="source">EDS accesstype</param>
        /// <param name="destination">protobuffer sdo access type</param>
        /// <param name="member">result object</param>
        /// <param name="context">resolve context</param>
        /// <returns>result </returns>
        public OdSubObject.Types.AccessSDO Convert(EDSsharp.AccessType source, OdSubObject.Types.AccessSDO destination, ResolutionContext context)
        {
            switch (source)
            {
                case EDSsharp.AccessType.rw:
                case EDSsharp.AccessType.rwr:
                case EDSsharp.AccessType.rww:
                    return OdSubObject.Types.AccessSDO.Rw;
                case EDSsharp.AccessType.ro:
                case EDSsharp.AccessType.@const:
                    return OdSubObject.Types.AccessSDO.Ro;
                case EDSsharp.AccessType.wo:
                    return OdSubObject.Types.AccessSDO.Wo;
                case EDSsharp.AccessType.UNKNOWN:
                default:
                    return OdSubObject.Types.AccessSDO.No;
            }
        }
        /// <summary>
        /// Resolver to convert eds access into PDO access type
        /// </summary>
        /// <param name="source">EDS accesstype</param>
        /// <param name="destination">protobuffer pdo access type</param>
        /// <param name="member">result object</param>
        /// <param name="context">resolve context</param>
        /// <returns>result </returns>
        public OdSubObject.Types.AccessPDO Convert(EDSsharp.AccessType source, OdSubObject.Types.AccessPDO destination, ResolutionContext context)
        {
            switch (source)
            {
                case EDSsharp.AccessType.rw:
                    return OdSubObject.Types.AccessPDO.Tr;
                case EDSsharp.AccessType.rwr:
                    return OdSubObject.Types.AccessPDO.T;
                case EDSsharp.AccessType.rww:
                case EDSsharp.AccessType.@const:
                    return OdSubObject.Types.AccessPDO.R;
                case EDSsharp.AccessType.ro:
                case EDSsharp.AccessType.wo:
                case EDSsharp.AccessType.UNKNOWN:
                default:
                    return OdSubObject.Types.AccessPDO.No;
            }
        }
        /// <summary>
        /// Resolver to convert SDO access type into eds access into 
        /// </summary>
        /// <param name="source">protobuffer sdo access type</param>
        /// <param name="destination">EDS accesstype</param>
        /// <param name="member">result object</param>
        /// <param name="context">resolve context</param>
        /// <returns>result </returns>
        public EDSsharp.AccessType Convert(OdSubObject source, EDSsharp.AccessType destination, ResolutionContext context)
        {
            if (source.Pdo == OdSubObject.Types.AccessPDO.Tr && source.Sdo == OdSubObject.Types.AccessSDO.Rw)
                return EDSsharp.AccessType.rw;
            else if (source.Pdo == OdSubObject.Types.AccessPDO.No && source.Sdo == OdSubObject.Types.AccessSDO.Ro)
                return EDSsharp.AccessType.ro;
            else if (source.Pdo == OdSubObject.Types.AccessPDO.No && source.Sdo == OdSubObject.Types.AccessSDO.Wo)
                return EDSsharp.AccessType.wo;
            else if (source.Pdo == OdSubObject.Types.AccessPDO.T && source.Sdo == OdSubObject.Types.AccessSDO.Rw)
                return EDSsharp.AccessType.rwr;
            else if (source.Pdo == OdSubObject.Types.AccessPDO.R && source.Sdo == OdSubObject.Types.AccessSDO.Rw)
                return EDSsharp.AccessType.rww;
            else if (source.Pdo == OdSubObject.Types.AccessPDO.R && source.Sdo == OdSubObject.Types.AccessSDO.Ro)
                return EDSsharp.AccessType.@const;
            else
                return EDSsharp.AccessType.UNKNOWN;
        }
    }

    public class ODStringToShortTypeResolver : ITypeConverter<string, UInt16>
    {
        /// <summary>
        /// Resolver to convert index & subindex string into short, will try hex, then descimal
        /// </summary>
        /// <param name="source">string containing index or subindex</param>
        /// <param name="destination">short int interpreted from the string</param>
        /// <param name="member">result object</param>
        /// <param name="context">resolve context</param>
        /// <returns>result </returns>
        public UInt16 Convert(string source, UInt16 destination, ResolutionContext context)
        {
            if (source.StartsWith("0x"))
            {
                var hex = source.Substring(2);
                return System.Convert.ToUInt16(hex, 16);
            }
            else
            {
                return System.Convert.ToUInt16(source);
            }
        }
    }
}
