using MKLP.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;

namespace MKLP.Functions
{
    public static class InventoryLogHandler
    {
        public static void Initialize(TerrariaPlugin plg)
        {
            GetDataHandlers.PlayerSlot += OnPlayerSlot;
        }
        public static void Dispose(TerrariaPlugin plg)
        {
            GetDataHandlers.PlayerSlot -= OnPlayerSlot;
        }

        public delegate void InventoryLog(InventoryLogArgs e);

        public static event InventoryLog InventoryLogEvent;
        public static async void OnInventoryLogEvent(InventoryLogArgs args)
        {
            if (InventoryLogEvent != null)
            {
                InventoryLogEvent(args);
            }
        }

        public class InventoryLogArgs
        {
            public TSPlayer Player { get; private set; }
            public NetItemKLP PreviousItem { get; private set; }
            public NetItemKLP CurrentItem { get; private set; }
            public int ItemSlot { get; private set; }
            public int ActualItemSlot { get; private set; }
            public InvSlotType ItemSlotType { get; private set; }
            public int ValueChanged { get; private set; }
            public ValueChangeType ValueChangedType { get; private set; }

            public InventoryLogArgs(TSPlayer player, NetItemKLP previousItem, NetItemKLP currentItem, int itemSlot, int actualItemSlot, InvSlotType itemSlotType, int valueChanged, ValueChangeType valueChangedType)
            {
                Player = player;
                PreviousItem = previousItem;
                CurrentItem = currentItem;
                ItemSlot = itemSlot;
                ActualItemSlot = actualItemSlot;
                ItemSlotType = itemSlotType;
                ValueChanged = valueChanged;
                ValueChangedType = valueChangedType;
            }
        }
        public enum ValueChangeType
        {
            Added,
            Removed,
            Decrease,
            Increase
        }
        private static void OnPlayerSlot(object? sender, GetDataHandlers.PlayerSlotEventArgs args)
        {
            TSPlayer player = args.Player;
            //short slot = args.Slot;
            //InvSlotType slotType = GetSlotType(slot, out int actualSlot);

            #region [ Get Data ]

            if (player.ContainsData("MKLP_PrevInventory"))
            {
                NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevInventory");

                for (int i = 0; i < player.TPlayer.inventory.Length; i++)
                {
                    NetItemKLP[] CurData = NetItemKLP.Convert_TItemArray(player.TPlayer.inventory);

                    if (PrevData[i].type != CurData[i].type ||
                        PrevData[i].stack != CurData[i].stack ||
                        PrevData[i].prefix != CurData[i].prefix)
                    {
                        Execute(PrevData[i], CurData[i], i, InvSlotType.Inventory);
                    }
                }
            }
            player.SetData("MKLP_PrevInventory", NetItemKLP.Convert_TItemArray(player.TPlayer.inventory));

            #region [ Armor Equip ]
            if (player.ContainsData("MKLP_PrevArmor"))
            {
                NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevArmor");

                for (int i = 0; i < player.TPlayer.armor.Length; i++)
                {
                    NetItemKLP[] CurData = NetItemKLP.Convert_TItemArray(player.TPlayer.armor);

                    if (PrevData[i].type != CurData[i].type ||
                        PrevData[i].stack != CurData[i].stack ||
                        PrevData[i].prefix != CurData[i].prefix)
                    {
                        Execute(PrevData[i], CurData[i], i, InvSlotType.Armor);
                    }
                }
            }
            player.SetData("MKLP_PrevArmor", NetItemKLP.Convert_TItemArray(player.TPlayer.armor));

            if (player.ContainsData("MKLP_PrevArmorDye"))
            {
                NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevArmorDye");

                for (int i = 0; i < player.TPlayer.dye.Length; i++)
                {
                    NetItemKLP[] CurData = NetItemKLP.Convert_TItemArray(player.TPlayer.dye);

                    if (PrevData[i].type != CurData[i].type ||
                        PrevData[i].stack != CurData[i].stack ||
                        PrevData[i].prefix != CurData[i].prefix)
                    {
                        Execute(PrevData[i], CurData[i], i, InvSlotType.ArmorDye);
                    }
                }
            }
            player.SetData("MKLP_PrevArmorDye", NetItemKLP.Convert_TItemArray(player.TPlayer.dye));
            #endregion

            #region [ Banks ( Extra Inv ) ]

            if (player.ContainsData("MKLP_PrevPiggyBank"))
            {
                NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevPiggyBank");

                for (int i = 0; i < player.TPlayer.bank.item.Length; i++)
                {
                    NetItemKLP[] CurData = NetItemKLP.Convert_TItemArray(player.TPlayer.bank.item);

                    if (PrevData[i].type != CurData[i].type ||
                        PrevData[i].stack != CurData[i].stack ||
                        PrevData[i].prefix != CurData[i].prefix)
                    {
                        Execute(PrevData[i], CurData[i], i, InvSlotType.PiggyBank);
                    }
                }
            }
            player.SetData("MKLP_PrevSafe", NetItemKLP.Convert_TItemArray(player.TPlayer.bank2.item));

            if (player.ContainsData("MKLP_PrevSafe"))
            {
                NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevSafe");

                for (int i = 0; i < player.TPlayer.bank2.item.Length; i++)
                {
                    NetItemKLP[] CurData = NetItemKLP.Convert_TItemArray(player.TPlayer.bank2.item);

                    if (PrevData[i].type != CurData[i].type ||
                        PrevData[i].stack != CurData[i].stack ||
                        PrevData[i].prefix != CurData[i].prefix)
                    {
                        Execute(PrevData[i], CurData[i], i, InvSlotType.Safe);
                    }
                }
            }
            player.SetData("MKLP_PrevSafe", NetItemKLP.Convert_TItemArray(player.TPlayer.bank2.item));

            if (player.ContainsData("MKLP_PrevDefenderForge"))
            {
                NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevDefenderForge");

                for (int i = 0; i < player.TPlayer.bank3.item.Length; i++)
                {
                    NetItemKLP[] CurData = NetItemKLP.Convert_TItemArray(player.TPlayer.bank3.item);

                    if (PrevData[i].type != CurData[i].type ||
                        PrevData[i].stack != CurData[i].stack ||
                        PrevData[i].prefix != CurData[i].prefix)
                    {
                        Execute(PrevData[i], CurData[i], i, InvSlotType.DefenderForge);
                    }
                }
            }
            player.SetData("MKLP_PrevDefenderForge", NetItemKLP.Convert_TItemArray(player.TPlayer.bank3.item));

            if (player.ContainsData("MKLP_PrevVoidVault"))
            {
                NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevVoidVault");

                for (int i = 0; i < player.TPlayer.bank4.item.Length; i++)
                {
                    NetItemKLP[] CurData = NetItemKLP.Convert_TItemArray(player.TPlayer.bank4.item);

                    if (PrevData[i].type != CurData[i].type ||
                        PrevData[i].stack != CurData[i].stack ||
                        PrevData[i].prefix != CurData[i].prefix)
                    {
                        Execute(PrevData[i], CurData[i], i, InvSlotType.VoidVault);
                    }
                }
            }
            player.SetData("MKLP_PrevVoidVault", NetItemKLP.Convert_TItemArray(player.TPlayer.bank4.item));
            #endregion

            #endregion


            bool Execute(NetItemKLP GetPreviousItem, NetItemKLP CurrentItem, int slot, InvSlotType slotType)
            {
                try
                {

                    int ValueChanged;
                    ValueChangeType ValueChangeType;

                    if (CurrentItem.type == 0 && GetPreviousItem.type != 0)
                    {
                        ValueChanged = GetPreviousItem.stack * -1;
                        ValueChangeType = ValueChangeType.Removed;
                    }
                    else if (GetPreviousItem.type == 0 && CurrentItem.type != 0)
                    {
                        ValueChanged = GetPreviousItem.stack;
                        ValueChangeType = ValueChangeType.Added;
                    }
                    else if (CurrentItem.stack > GetPreviousItem.stack)
                    {
                        ValueChanged = CurrentItem.stack - GetPreviousItem.stack;
                        ValueChangeType = ValueChangeType.Increase;
                    }
                    else if (CurrentItem.stack < GetPreviousItem.stack)
                    {
                        ValueChanged = CurrentItem.stack - GetPreviousItem.stack;
                        ValueChangeType = ValueChangeType.Decrease;
                    }
                    else
                    {
                        return false;
                    }

                    InventoryLogArgs getargs = new InventoryLogArgs(
                        player,
                        GetPreviousItem,
                        CurrentItem,
                        -1, //global slot
                        slot,//actual slot
                        slotType,
                        ValueChanged,
                        ValueChangeType
                        );

                    OnInventoryLogEvent(getargs);
                    return false;
                }
                catch (Exception e)
                {
                    MKLP_Console.SendLog_Exception(e);
                    return false;
                }

            }
        }
        //Old
        /*
        private static void OnPlayerSlot(object? sender, GetDataHandlers.PlayerSlotEventArgs args)
        {
            TSPlayer player = args.Player;
            short slot = args.Slot;
            InvSlotType slotType = GetSlotType(slot, out int actualSlot);

            NetItemKLP CurrentItem = new NetItemKLP(args.Type, args.Stack, args.Prefix);

            #region [ Get Data ]
            switch (slotType)
            {
                case InvSlotType.Inventory:
                    {
                        if (player.ContainsData("MKLP_PrevInventory"))
                        {
                            NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevInventory");
                            args.Handled = Execute(PrevData);
                        }
                        player.SetData("MKLP_PrevInventory", NetItemKLP.Convert_TItemArray(player.TPlayer.inventory));

                        return;
                    }
                case InvSlotType.Armor:
                case InvSlotType.ArmorDye:
                    {
                        actualSlot = slot - NetItem.MiscEquipIndex.Item1;
                        if (player.ContainsData("MKLP_PrevArmor"))
                        {
                            NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevArmor");
                            args.Handled = Execute(PrevData);
                        }
                        player.SetData("MKLP_PrevArmor",
                            Combine_NetItemKLPArrays(
                                NetItemKLP.Convert_TItemArray(player.TPlayer.armor),
                                NetItemKLP.Convert_TItemArray(player.TPlayer.dye)
                                ));

                        return;
                    }
                #region [ Banks ( Extra Inv ) ]
                case InvSlotType.PiggyBank:
                    {
                        if (player.ContainsData("MKLP_PrevPiggyBank"))
                        {
                            NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevPiggyBank");
                            args.Handled = Execute(PrevData);
                        }
                        player.SetData("MKLP_PrevPiggyBank", NetItemKLP.Convert_TItemArray(player.TPlayer.bank.item));

                        return;
                    }
                case InvSlotType.Safe:
                    {
                        if (player.ContainsData("MKLP_PrevSafe"))
                        {
                            NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevSafe");
                            args.Handled = Execute(PrevData);
                        }
                        player.SetData("MKLP_PrevSafe", NetItemKLP.Convert_TItemArray(player.TPlayer.bank2.item));

                        return;
                    }
                case InvSlotType.DefenderForge:
                    {
                        if (player.ContainsData("MKLP_PrevDefenderForge"))
                        {
                            NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevDefenderForge");
                            args.Handled = Execute(PrevData);
                        }
                        player.SetData("MKLP_PrevDefenderForge", NetItemKLP.Convert_TItemArray(player.TPlayer.bank3.item));

                        return;
                    }
                case InvSlotType.VoidVault:
                    {
                        if (player.ContainsData("MKLP_PrevVoidVault"))
                        {
                            NetItemKLP[] PrevData = player.GetData<NetItemKLP[]>("MKLP_PrevVoidVault");
                            args.Handled = Execute(PrevData);
                        }
                        player.SetData("MKLP_PrevVoidVault", NetItemKLP.Convert_TItemArray(player.TPlayer.bank4.item));

                        return;
                    }
                    #endregion
            }
            #endregion


            bool Execute(NetItemKLP[] PreviousItems)
            {
                try
                {
                    NetItemKLP GetPreviousItem = PreviousItems[actualSlot];

                    int ValueChanged;
                    ValueChangeType ValueChangeType;

                    if (CurrentItem.type == 0 && GetPreviousItem.type != 0)
                    {
                        ValueChanged = GetPreviousItem.stack * -1;
                        ValueChangeType = ValueChangeType.Removed;
                    }
                    else if (GetPreviousItem.type == 0 && CurrentItem.type != 0)
                    {
                        ValueChanged = GetPreviousItem.stack;
                        ValueChangeType = ValueChangeType.Added;
                    }
                    else if (CurrentItem.stack > GetPreviousItem.stack)
                    {
                        ValueChanged = CurrentItem.stack - GetPreviousItem.stack;
                        ValueChangeType = ValueChangeType.Increase;
                    }
                    else if (CurrentItem.stack < GetPreviousItem.stack)
                    {
                        ValueChanged = CurrentItem.stack - GetPreviousItem.stack;
                        ValueChangeType = ValueChangeType.Decrease;
                    }
                    else
                    {
                        return false;
                    }

                    InventoryLogArgs getargs = new InventoryLogArgs(
                        player,
                        GetPreviousItem,
                        CurrentItem,
                        slot,
                        actualSlot,
                        slotType,
                        ValueChanged,
                        ValueChangeType
                        );

                    OnInventoryLogEvent(getargs);
                    return false;
                }
                catch (Exception e)
                {
                    MKLP_Console.SendLog_Exception(e);
                    return false;
                }

            }
        }
        */
        public static NetItemKLP[] Combine_NetItemKLPArrays(params NetItemKLP[][] arr)
        {
            List<NetItemKLP> result = new();

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != null)
                {
                    result.AddRange(arr[i]);
                }
            }

            return result.ToArray();
        }

        public static InvSlotType GetSlotType(int inventorySlot, out int ActualSlot)
        {
            ActualSlot = -1;
            if (inventorySlot < NetItem.InventoryIndex.Item2)
            {
                ActualSlot = inventorySlot;
                return InvSlotType.Inventory;
            }
            else if (inventorySlot < NetItem.ArmorIndex.Item2)
            {
                ActualSlot = inventorySlot - NetItem.ArmorIndex.Item1;
                return InvSlotType.Armor;
            }
            else if (inventorySlot < NetItem.DyeIndex.Item2)
            {
                ActualSlot = inventorySlot - NetItem.DyeIndex.Item1;
                return InvSlotType.ArmorDye;
            }
            else if (inventorySlot < NetItem.MiscEquipIndex.Item2)
            {
                ActualSlot = inventorySlot - NetItem.MiscEquipIndex.Item1;
                return InvSlotType.Equipment;
            }
            else if (inventorySlot < NetItem.MiscDyeIndex.Item2)
            {
                ActualSlot = inventorySlot - NetItem.MiscDyeIndex.Item1;
                return InvSlotType.EquipmentDye;
            }
            else if (inventorySlot < NetItem.PiggyIndex.Item2)
            {
                ActualSlot = inventorySlot - NetItem.PiggyIndex.Item1;
                return InvSlotType.PiggyBank;
            }
            else if (inventorySlot < NetItem.SafeIndex.Item2)
            {
                ActualSlot = inventorySlot - NetItem.SafeIndex.Item1;
                return InvSlotType.Safe;
            }
            else if (inventorySlot < NetItem.TrashIndex.Item2)
            {
                ActualSlot = 0;
                return InvSlotType.TrashSlot;
            }
            else if (inventorySlot < NetItem.ForgeIndex.Item2)
            {
                ActualSlot = inventorySlot - NetItem.ForgeIndex.Item1;
                return InvSlotType.DefenderForge;
            }
            else if (inventorySlot < NetItem.VoidIndex.Item2)
            {
                ActualSlot = inventorySlot - NetItem.VoidIndex.Item1;
                return InvSlotType.VoidVault;
            }
            else if (inventorySlot < NetItem.Loadout1Armor.Item2)
            {
                ActualSlot = inventorySlot - NetItem.Loadout1Armor.Item1;
                return InvSlotType.Loadout1;
            }
            else if (inventorySlot < NetItem.Loadout1Dye.Item2)
            {
                ActualSlot = inventorySlot - NetItem.Loadout1Dye.Item1;
                return InvSlotType.LoadoutDye1;
            }
            else if (inventorySlot < NetItem.Loadout2Armor.Item2)
            {
                ActualSlot = inventorySlot - NetItem.Loadout2Armor.Item1;
                return InvSlotType.Loadout2;
            }
            else if (inventorySlot < NetItem.Loadout2Dye.Item2)
            {
                ActualSlot = inventorySlot - NetItem.Loadout2Dye.Item1;
                return InvSlotType.LoadoutDye2;
            }
            else if (inventorySlot < NetItem.Loadout3Armor.Item2)
            {
                ActualSlot = inventorySlot - NetItem.Loadout3Armor.Item1;
                return InvSlotType.Loadout3;
            }
            else if (inventorySlot < NetItem.Loadout3Dye.Item2)
            {
                ActualSlot = inventorySlot - NetItem.Loadout3Dye.Item1;
                return InvSlotType.LoadoutDye3;
            }

            return InvSlotType.Unknown;
        }
        public enum InvSlotType
        {
            Unknown,
            Inventory,
            TrashSlot,
            Armor,
            ArmorDye,
            Equipment,
            EquipmentDye,
            PiggyBank,
            Safe,
            DefenderForge,
            VoidVault,
            Loadout1,
            LoadoutDye1,
            Loadout2,
            LoadoutDye2,
            Loadout3,
            LoadoutDye3
        }
    }
}
