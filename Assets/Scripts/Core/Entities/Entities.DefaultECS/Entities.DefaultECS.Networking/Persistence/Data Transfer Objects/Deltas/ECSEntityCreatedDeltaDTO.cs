﻿using System;

namespace HereticalSolutions.Entities
{
    [Serializable]
    public struct ECSEntityCreatedDeltaDTO
    {
        public string EntityGUID;
        
        public string PrototypeID;
    }
}