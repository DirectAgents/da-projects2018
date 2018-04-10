using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Domain.DTO
{
    //TODO? make an IABStat interface and change all referencing code to use that?
    //    ? make ABStat inherit from ABLineItem ?

    // "Accounting Backup" stats - for the dashboard
    public class ABStat
    {
        private const int NUM_DECIMALS_FOR_ROUNDING = 5;

        public ABStat() { }
        public ABStat(IEnumerable<IRTLineItem> rtLineItems)
        {
            //Id = ?
            //Client = ?
            Rev = Decimal.Round(rtLineItems.Sum(li => li.Revenue), 2);
            Cost = Decimal.Round(rtLineItems.Sum(li => li.Cost), 2);
        }
        public ABStat(IRTLineItem rtLineItem)
        {
            //Id = ?ABId / RTId?
            Name = rtLineItem.Name;
            Client = rtLineItem.Name; // ?
            Rev = Decimal.Round(rtLineItem.Revenue, 2);
            Cost = Decimal.Round(rtLineItem.Cost, 2);
        }

        //TODO: RTId ?  (make nullable...? Id? ABId?)

        public int Id { get; set; }

        public string Name { get; set; }

        public string Client { get; set; } // ?
        //public string Campaign { get; set; }
        //public string Vendor { get; set; }

        public decimal Rev { get; set; }
        public decimal Cost { get; set; }
        public decimal Margin
        {
            get { return Rev - Cost; }
        }

        public decimal Budget { get; set; }
        public decimal OverBudget // (Amount over budget)
        {
            get { return Rev - Budget; }
        }
        public decimal FractionBudget // (Fraction of budget used)
        {
            get { return (Budget == 0) ? 0 : Decimal.Round(Rev / Budget, NUM_DECIMALS_FOR_ROUNDING); }
        }

        public decimal StartBal { get; set; }
        public decimal CurrBal
        {
            get { return StartBal - Rev; }
        }
        public decimal ExtCred { get; set; } // this one: usually larger
        public decimal IntCred { get; set; } // this one: provides cushion
    }

    public class ABLineItem
    {
        public ABLineItem(IRTLineItem rtLineItem)
        {
            Name = rtLineItem.Name;
            Rev = Decimal.Round(rtLineItem.Revenue, 2);
            Cost = Decimal.Round(rtLineItem.Cost, 2);
            RevCurr = rtLineItem.RevCurr;
            CostCurr = rtLineItem.CostCurr;
            Units = rtLineItem.Units;
            RevPerUnit = rtLineItem.RevPerUnit;
        }
        public ABLineItem(ABExtraItem extraItem)
        {
            Name = extraItem.Description;
            Rev = extraItem.Revenue;
            Cost = extraItem.Cost;
        }
        public ABLineItem(ProtoSpendBit spendBit)
        {
            Name = spendBit.ProtoCampaign.Name;
            Rev = spendBit.Revenue;
            Note = spendBit.Desc;
        }

        public string Name { get; set; }
        public string Note { get; set; }
        public decimal Rev { get; set; }
        public decimal Cost { get; set; }
        public decimal Margin
        {
            get { return Rev - Cost; }
        }

        public string RevCurr { get; set; }
        public string CostCurr { get; set; }
        public decimal? Units { get; set; }
        public decimal? RevPerUnit { get; set; }
    }
}
