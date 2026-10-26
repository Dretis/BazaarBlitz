using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [System.Serializable]
    public struct BoardNodeData
    {
        public int Id; // { get; private set; }
        public NodeType Type;
        [Space]
        public int DeployableOwnerId;
        public Deployable Deployable;
        [Space]
        public List<int> occupiedEntities;

        [Header("Nearby Nodes")]
        public NearbyNodeData NorthNode;
        public NearbyNodeData EastNode;
        public NearbyNodeData SouthNode;
        public NearbyNodeData WestNode;

        [Header("Type-Specific Data")]
        public StorefrontData StorefrontData;
        public StampData StampData;
    }

    #region Type-Specific Structs
    [System.Serializable]
    public struct NearbyNodeData
    {
        public int Id;
        public bool IsOneWay;
    }

    [System.Serializable]
    public struct StorefrontData
    {
        //public EntityData Owner;
        public int OwnerId;
        public const int STORESTOCK_CAP = 3;
        public ItemStats[] Storestock;

        public int StoreLevel; // = 1;
        public float StorePriceMultiplier; // = 1;
        public float StoreBaseMult; // = 0.2f;
        public int StoreHeldCoins; // = 0;
        
        public StorefrontData(PlayerState ps)
        {
            //Owner = ed;
            OwnerId = ps.Id;

            Storestock = new ItemStats[STORESTOCK_CAP];
            StoreLevel = 1;
            StorePriceMultiplier = 1;
            StoreBaseMult = 0.2f;
            StoreHeldCoins = 0;
        }
        public StorefrontData(StorefrontData sd)
        {
            OwnerId = sd.OwnerId;

            Storestock = sd.Storestock;
            StoreLevel = sd.StoreLevel;
            StorePriceMultiplier = sd.StorePriceMultiplier;
            StoreBaseMult = sd.StoreBaseMult;
            StoreHeldCoins = sd.StoreHeldCoins;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < Storestock.Length; i++)
            {
                if (Storestock[i] != null ) return false;
            }

            return true;
        }
    }

    [System.Serializable]
    public struct StampData
    {
        public StampType CurrentStampType;
        //public int CurrentStampId;

        public StampType GetNextStampType()
        {
            var nextId = (int)CurrentStampType;
            nextId++;

            if (nextId > 4)
            {
                nextId = 1;
            }

            //CurrentStampId = nextId;
            //CurrentStampType = (StampType)nextId;
            return (StampType)nextId;
        }
        public StampType GetPrevStampType()
        {
            var nextId = (int)CurrentStampType;
            nextId--;

            if (nextId < 1)
            {
                nextId = 4;
            }

            //CurrentStampId = nextId;
            //CurrentStampType = (StampType)nextId;
            return (StampType)nextId;
        }
    }

    [System.Serializable]
    public struct WildcardData
    {
        // unneeded?
    }
    #endregion

    public enum NodeType
    {
        None,
        OpenSpace,
        Storefront,
        Exchange, // Castle
        Stamp,
        Wildcard,
        MoveAgain,
        Warp,
        Vendor,
        LuckyDice,
        CoconutTree
    }

    public enum Deployable
    {
        None,
        HoaMai, // was Marigold
        Rafflesia,
        Bomb
    }
    
    public enum StampType
    {
        None,
        Green,
        Red,
        Blue,
        Orange
    }
}

