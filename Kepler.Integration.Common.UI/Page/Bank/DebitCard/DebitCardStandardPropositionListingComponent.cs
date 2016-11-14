﻿using System;
using System.Collections.Generic;
using System.Linq;
using Kepler.Integration.Common.UI.Page.Bank.Product;
using Kepler.Integration.Common.UI.SelenoExtension;
using OpenQA.Selenium;

namespace Kepler.Integration.Common.UI.Page.Bank.DebitCard
{
    public class DebitCardStandardPropositionListingComponent : StandardPropositionListingComponent<DebitCardPropositionItem>,
        IResult<DebitCardPropositionComponent, DebitCardPropositionItem>
    {
        public new IEnumerable<DebitCardPropositionComponent> GetPropositionsResult()
        {
            var resultElements =
                Find.Elements(
                    By.CssSelector("div.standard-offers-test > .results-container > li.results-container-line"),
                    TimeSpan.FromSeconds(3)).Where(item => item.GetAttribute("class").Contains("T-Vklady-TGB") == false);

            return InitResultComponents<DebitCardPropositionComponent>(resultElements);
        }

        public override void SelectSortingType(string sortOption)
        {
            GetSelectedSortingElement().Click();

            var selectedSortingElement =
                GetDriver().WaitAllElementsAndReturnExpected(By.CssSelector("div.select-control-inner--sorting-dd li.ik_select_option"),
                    element => element.Displayed && element.FindElement(By.CssSelector("span")).Text == sortOption);

            selectedSortingElement.Click();
        }
    }
}