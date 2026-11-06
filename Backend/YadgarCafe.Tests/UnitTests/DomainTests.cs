using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Tests.UnitTests
{
    public class IngredientTests
    {
        private static Ingredient Milk(decimal stock)
        {
            var i = new Ingredient { Name = "Milk", Unit = "L" };
            i.ApplyMovement(StockMovementType.Purchase, stock, null);
            return i;
        }

        [Fact]
        public void ApplyMovement_updates_balance_and_returns_ledger_entry()
        {
            var milk = Milk(10);

            var entry = milk.ApplyMovement(StockMovementType.OrderConsumption, -4, "order");

            Assert.Equal(6, milk.CurrentStock);
            Assert.Equal(-4, entry.Quantity);
            Assert.Equal(6, entry.BalanceAfter);
        }

        [Fact]
        public void ApplyMovement_rejects_negative_stock_and_leaves_balance_untouched()
        {
            var milk = Milk(3);

            Assert.Throws<ConflictException>(() =>
                milk.ApplyMovement(StockMovementType.OrderConsumption, -5, null));

            Assert.Equal(3, milk.CurrentStock);
        }

        [Fact]
        public void IsLowStock_is_true_at_or_below_minimum()
        {
            var milk = Milk(5);
            milk.MinimumStock = 5;
            Assert.True(milk.IsLowStock);

            milk.MinimumStock = 4;
            Assert.False(milk.IsLowStock);
        }
    }

    public class OrderTests
    {
        private static Order NewOrder()
        {
            var order = new Order();
            order.AddItem(new Product { Name = "Chai", Price = 20 }, 2);
            order.AddItem(new Product { Name = "Samosa", Price = 15 }, 1);
            return order;
        }

        [Fact]
        public void AddItem_computes_totals_and_snapshots_name()
        {
            var order = NewOrder();

            Assert.Equal(55, order.TotalAmount);
            Assert.Contains(order.OrderItems, i => i.ProductName == "Chai" && i.TotalPrice == 40);
        }

        [Fact]
        public void AddItem_rejects_non_positive_quantity()
        {
            Assert.Throws<BadRequestException>(() => new Order().AddItem(new Product { Price = 1 }, 0));
        }

        [Fact]
        public void Status_follows_the_allowed_flow()
        {
            var order = NewOrder();

            order.ChangeStatus(OrderStatus.Preparing);
            order.ChangeStatus(OrderStatus.Ready);
            order.ChangeStatus(OrderStatus.Completed);

            Assert.Equal(OrderStatus.Completed, order.Status);
        }

        [Fact]
        public void Status_cannot_skip_steps()
        {
            var order = NewOrder();
            Assert.Throws<ConflictException>(() => order.ChangeStatus(OrderStatus.Completed));
        }

        [Fact]
        public void Completed_order_cannot_be_cancelled()
        {
            var order = NewOrder();
            order.ChangeStatus(OrderStatus.Preparing);
            order.ChangeStatus(OrderStatus.Ready);
            order.ChangeStatus(OrderStatus.Completed);

            Assert.Throws<ConflictException>(() => order.Cancel());
        }

        [Fact]
        public void Cancelling_a_paid_order_marks_it_refunded()
        {
            var order = NewOrder();
            order.ChangePaymentStatus(PaymentStatus.Paid);

            order.Cancel();

            Assert.Equal(OrderStatus.Cancelled, order.Status);
            Assert.Equal(PaymentStatus.Refunded, order.PaymentStatus);
        }

        [Fact]
        public void Payment_cannot_change_on_a_cancelled_order()
        {
            var order = NewOrder();
            order.Cancel();

            Assert.Throws<ConflictException>(() => order.ChangePaymentStatus(PaymentStatus.Paid));
        }
    }
}
