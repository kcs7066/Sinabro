using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using System; // ## Action 사용을 위해 추가 ##

public struct InventorySlot : INetworkSerializable, IEquatable<InventorySlot>
{
    public int itemID;
    public int quantity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemID);
        serializer.SerializeValue(ref quantity);
    }

    public bool Equals(InventorySlot other)
    {
        return itemID == other.itemID && quantity == other.quantity;
    }
}

public class PlayerInventory : NetworkBehaviour
{
    public static PlayerInventory LocalInstance { get; private set; }

    // ## 수정: 로컬 인스턴스가 준비되었음을 알리는 '방송'을 추가합니다. ##
    public static event Action<PlayerInventory> OnLocalInstanceReady;

    public NetworkList<InventorySlot> inventorySlots;

    private void Awake()
    {
        inventorySlots = new NetworkList<InventorySlot>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            // ## 수정: 준비되었음을 방송합니다. ##
            OnLocalInstanceReady?.Invoke(this);
            Debug.Log("[Client] PlayerInventory.LocalInstance has been set and event has been invoked!");
        }
    }

    // ## AddItem, RemoveItem, GetItemQuantity 함수는 변경사항이 없습니다. ##
    public void AddItem(int itemID, int amount)
    {
        if (!IsServer) return;

        bool itemExists = false;
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].itemID == itemID)
            {
                InventorySlot updatedSlot = inventorySlots[i];
                updatedSlot.quantity += amount;
                inventorySlots[i] = updatedSlot;
                itemExists = true;
                break;
            }
        }

        if (!itemExists)
        {
            inventorySlots.Add(new InventorySlot { itemID = itemID, quantity = amount });
        }
    }

    public void RemoveItem(int itemID, int amount)
    {
        if (!IsServer) return;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].itemID == itemID)
            {
                InventorySlot updatedSlot = inventorySlots[i];
                updatedSlot.quantity -= amount;

                if (updatedSlot.quantity <= 0)
                {
                    inventorySlots.RemoveAt(i);
                }
                else
                {
                    inventorySlots[i] = updatedSlot;
                }
                break;
            }
        }
    }

    public int GetItemQuantity(int itemID)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.itemID == itemID)
            {
                return slot.quantity;
            }
        }
        return 0;
    }
}

