
using System;

namespace BusinessEntity
{
    /// <summary>
    ///  醒燈特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    
    public class EntityAttribute : Attribute
    {
        private EntityType entityType;
        private string resourceID;

        public EntityAttribute(EntityType p_EntityType,string p_ResourceID)
        {
            this.entityType = p_EntityType;
            this.resourceID = p_ResourceID;
        }

        public EntityType EntityType
        {
            get
            {
                return entityType; 
            }
        }

        public string ResourceID
        {
            get
            {
                return resourceID;
            }
        }
    }
   
    public enum EntityType{Normal,Operation}
}
