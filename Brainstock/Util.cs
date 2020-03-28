using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

//Used pongo1231 script for reference
//His github has been a big help for me learning C#
//https://github.com/pongo1231

namespace Brainstock.Util
{
    //Register, set, get Decorators for Network
    public static class EntityDecor
    {
        public enum DecorType
        {
            Float = 1,
            Bool,
            Int,
            Time = 5
        }

        public static bool HasDecor(Entity ent, string name)
        {
            return Function.Call<bool>(Hash.DECOR_EXIST_ON, ent.NativeValue, name);
        }

        public static void RegisterDecorProperty(string name, DecorType type)
        {
            Function.Call(Hash.DECOR_REGISTER, name, (int)type);
        }

        public static void SetDecorBool(Entity ent, string name, bool value )
        {
            Function.Call(Hash.DECOR_SET_BOOL, ent.NativeValue, name, value);
            DecorSetBool(ent.Handle, name, value);
        }

        public static void SetDecorInt(Entity ent, string name, int value )
        {
            Function.Call(Hash.DECOR_SET_INT, ent.NativeValue, name, value);
            DecorSetInt(ent.Handle, name, value);
            //Debug.WriteLine($"DECORSET: {name} [{ent.NativeValue}] -> {value}");
        }

        public static void SetDecorFloat(Entity ent, string name, float value)
        {
            Function.Call(Hash._DECOR_SET_FLOAT, ent.NativeValue, name, value);
        }

        public static bool GetDecorBool(Entity ent, string name)
        {
            return (bool)DecorGetBool(ent.Handle, name);
        }

        public static int GetDecorInt(Entity ent, string name)
        {
            int dec = DecorGetInt(ent.Handle, name);
            //Debug.WriteLine($"DECORGET: {name} [{ent.GetHashCode()}] -> {dec}");
            return dec;
        }

        public static float GetDecorFloat(Entity ent, string name)
        {
            return (float)DecorGetFloat(ent.Handle, name);
        }
    }

    public static class EntityGet
    {
        public static Ped GetPedByID(int id)
        {
            foreach (Ped ped in World.GetAllPeds())
            {
                if (id == ped.GetHashCode())
                    return ped;
            }
            return null;
        }

        public static uint GetVehicleHashFromName(string name)
        {
            foreach (VehicleHash veh in Enum.GetValues(typeof(VehicleHash)))
            {
                uint hash = (uint)veh;
                string displayName = GetDisplayNameFromVehicleModel(hash);

                displayName = displayName.ToUpper();
                name = name.ToUpper();

                //Debug.WriteLine(displayName);
                if (displayName.Equals(name))
                {
                    return hash;
                }
            }
            return 0;
        }
    }

    public static class EntityCreate
    {
        public static async Task<Ped> CreatePed(Model model, int pedtype, Vector3 pos, float heading)
        {
            uint hash = (uint)model.Hash;

            if (!IsModelAPed(hash))
                return null;

            RequestModel(hash);

            while (!HasModelLoaded(hash))
                await BaseScript.Delay(1);

            Ped ped = new Ped(API.CreatePed((int)pedtype, hash, pos.X, pos.Y, pos.Z, heading, true, false));

            model.MarkAsNoLongerNeeded();

            return ped;
        }

        public static async Task<Prop> CreateProp(string model, Vector3 pos, bool dynamic = true, bool OnGround = true)
        {
            uint hash = (uint)GetHashKey(model);

            if (!IsModelValid(hash))
                return null;

            RequestModel(hash);
            while (!HasModelLoaded(hash))
                await BaseScript.Delay(1);

            if (OnGround)
                pos.Z = World.GetGroundHeight(pos);

            //Prop prop = new Prop( CreateObjectNoOffset(hash, pos.X, pos.Y, pos.X, networked, true, dynamic) );
            Prop prop = new Prop(CreateObject((int)hash, pos.X, pos.Y, pos.Z, true, true, dynamic));
            prop.PositionNoOffset = pos;

            SetModelAsNoLongerNeeded(hash);

            EntityDecor.SetDecorBool(prop, "_BRAINSTOCK", true);
            return prop;
        }

       public static async Task<Vehicle> CreateVehicle(uint hash, Vector3 pos, float heading, bool networked = true)
        {
            if (!IsModelInCdimage(hash))
                return null;

            RequestModel(hash);

            while (!HasModelLoaded(hash))
                await BaseScript.Delay(1);

            Vehicle veh = new Vehicle(API.CreateVehicle(hash, pos.X, pos.Y, pos.Z, heading, true, true));
            SetModelAsNoLongerNeeded(hash);

            EntityDecor.SetDecorBool(veh, "_BRAINSTOCK", true);

            return veh;
        }
        
        public static async Task<bool> SetSkin(string mdlName, int playerID)
        {
            uint mdlHash = (uint)GetHashKey(mdlName);

            if (!IsModelInCdimage(mdlHash))
                return false;

            RequestModel(mdlHash);
            while (!HasModelLoaded(mdlHash))
                await BaseScript.Delay(1);

            SetPlayerModel(playerID, mdlHash);
            SetModelAsNoLongerNeeded(mdlHash);

            int ped = GetPlayerPed(playerID);
            while (GetEntityModel(ped) != mdlHash)
                return false;

            return true;
        }
    }
}
