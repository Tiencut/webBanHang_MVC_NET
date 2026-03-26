using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SV22T1020761.Shop.AppCodes
{
    public static class SessionExtensions
    {
        public static void SetSessionData<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T GetSessionData<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }

    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public string Photo { get; set; }
    }

    public static class CartHelper
    {
        private const string CartSessionKey = "Cart";

        public static List<CartItem> GetCart(ISession session)
        {
            return session.GetSessionData<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        public static void AddToCart(ISession session, CartItem item)
        {
            var cart = GetCart(session);
            var existingItem = cart.Find(ci => ci.ProductID == item.ProductID);
            if (existingItem != null)
            {
                existingItem.Qty += item.Qty;
            }
            else
            {
                cart.Add(item);
            }
            session.SetSessionData(CartSessionKey, cart);
        }

        public static void UpdateCartItem(ISession session, int productId, int qty)
        {
            var cart = GetCart(session);
            var item = cart.Find(ci => ci.ProductID == productId);
            if (item != null)
            {
                if (qty > 0)
                {
                    item.Qty = qty;
                }
                else
                {
                    cart.Remove(item);
                }
                session.SetSessionData(CartSessionKey, cart);
            }
        }

        public static void RemoveFromCart(ISession session, int productId)
        {
            var cart = GetCart(session);
            var item = cart.Find(ci => ci.ProductID == productId);
            if (item != null)
            {
                cart.Remove(item);
                session.SetSessionData(CartSessionKey, cart);
            }
        }

        public static (int Count, decimal Total) GetCartSummary(ISession session)
        {
            var cart = GetCart(session);
            int count = 0;
            decimal total = 0;

            foreach (var item in cart)
            {
                count += item.Qty;
                total += item.Price * item.Qty;
            }

            return (count, total);
        }

        public static void ClearCart(ISession session)
        {
            session.Remove(CartSessionKey);
        }
    }
}