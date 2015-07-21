﻿using AntShares.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AntShares.Core
{
    public class AgencyTransaction : Transaction
    {
        public Order[] Orders;

        public AgencyTransaction()
            : base(TransactionType.AgencyTransaction)
        {
        }

        protected override void DeserializeExclusiveData(BinaryReader reader)
        {
            this.Orders = reader.ReadSerializableArray<Order>();
            if (Orders.Length < 2) throw new FormatException();
            if (Orders.Select(p => p.Agent).Distinct().Count() != 1)
                throw new FormatException();
        }

        public override IEnumerable<TransactionInput> GetAllInputs()
        {
            return Orders.SelectMany(p => p.Inputs).Concat(base.GetAllInputs());
        }

        public override UInt160[] GetScriptHashesForVerifying()
        {
            return base.GetScriptHashesForVerifying().Union(new UInt160[] { Orders[0].Agent }).OrderBy(p => p).ToArray();
        }

        protected override void SerializeExclusiveData(BinaryWriter writer)
        {
            writer.Write(Orders);
        }

        public override bool Verify()
        {
            if (!base.Verify()) return false;
            if (Orders.Length < 2) return false;
            foreach (Order order in Orders)
                if (!order.Verify())
                    return false;
            return true;
        }

        internal override bool VerifyBalance()
        {
            //TODO: 验证合法性
            //1. 输入输出
            //2. 成交是否符合每一个订单的要求（价格、数量等）
        }
    }
}
