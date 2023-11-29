/*
* $Author:dagger.li
* $Date:2023/7/10
*/
using System;
using System.Collections.Generic;
using System.Text;
using BusinessEntity;
using System.Threading.Tasks;
namespace BusinessFacade
{
    public interface IVersions : Orleans.IGrainWithIntegerKey
    {
        Task<VersionsEntity> Create(VersionsEntity model);
        Task<VersionsEntity> Updates(VersionsEntity model);
        Task<VersionsEntity> Confirm(VersionsEntity model);

        Task<VersionsEntity> Cancel(VersionsEntity model);

        Task<VersionsEntity> GetModel(decimal Version);
    }
}