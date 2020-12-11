﻿using System;
using System.Collections.Generic;
using System.Text;
using Nest;
using OptionConfig;

namespace ElasticSearch7
{
    /// <summary>
    /// Es基础实体
    /// </summary>
    public abstract class EsBaseDataMapping<T> where T : class, new()
    {
        /// <summary>
        /// 新索引别名
        /// </summary>
        public virtual string AliasIndex { get; set; } = "";

        /// <summary>
        /// 映射并创建索引类型
        /// </summary>
        public virtual EsMappingType EsMappingType { get; set; } = EsMappingType.Default;

        /// <summary>
        /// 分片数
        /// </summary>
        public virtual int NumberOfShards => 5;

        /// <summary>
        /// 获取当前索引
        /// </summary>
        public string CurrentIndex => GetIndex(DateTime.Now);

        /// <summary>
        /// 当前Es客户端
        /// </summary>
        public ElasticClient EsClient
        {
            get
            {
                var currClient = EsClientProvider.GetClient();
                var exists = currClient.Indices.Exists(CurrentIndex).Exists;
                if (!exists) { IndexCreateAndMapping(); }
                return currClient;
            }
        }

        /// <summary>
        /// 获取指定时间索引
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public string GetIndex(DateTime dateTime)
        {
            if (string.IsNullOrEmpty(AliasIndex))
            {
                EsMappingType = EsMappingType.Default;
            }
            else if (EsMappingType == EsMappingType.Default)
            {
                EsMappingType = EsMappingType.New;
            }
            switch (EsMappingType)
            {
                case EsMappingType.Default:
                    return ElasticSearchConfig.EsDefaultIndex;
                case EsMappingType.New:
                    return AliasIndex;
                case EsMappingType.Hour:
                    return $"{AliasIndex}_{dateTime:yyyyMMddHH}";
                case EsMappingType.Day:
                    return $"{AliasIndex}_{dateTime:yyyyMMdd}";
                case EsMappingType.Month:
                    return $"{AliasIndex}_{dateTime:yyyyMM}";
                case EsMappingType.Year:
                    return $"{AliasIndex}_{dateTime:yyyy}";
                default:
                    return ElasticSearchConfig.EsDefaultIndex;
            }
        }

        /// <summary>
        /// 创建指定时间索引
        /// </summary>
        private void IndexCreateAndMapping()
        {
            var esMappingSettings = new EsCreateIndexSettings()
            {
                NumberOfShards = NumberOfShards,
                AliasIndex = AliasIndex,
                Index = CurrentIndex
            };
            EsClientProvider.CreateIndex(esMappingSettings, EntityMapping);
        }

        /// <summary>
        /// 实体映射
        /// </summary>
        /// <param name="client">es客户端</param>
        /// <param name="index">索引名称</param>
        public virtual void EntityMapping(ElasticClient client, string index)
        {
            client.Map<T>(m => m.AutoMap().Index(index));
        }
    }

    /// <summary>
    /// 映射并创建索引类型
    /// </summary>
    public enum EsMappingType
    {
        /// <summary>
        /// 默认索引
        /// </summary>
        Default,
        /// <summary>
        /// 新创建索引
        /// </summary>
        New,
        /// <summary>
        /// 按小时创建索引
        /// </summary>
        Hour,
        /// <summary>
        /// 按天创建索引
        /// </summary>
        Day,
        /// <summary>
        /// 按月创建索引
        /// </summary>
        Month,
        /// <summary>
        /// 按年创建索引
        /// </summary>
        Year,
    }
}
