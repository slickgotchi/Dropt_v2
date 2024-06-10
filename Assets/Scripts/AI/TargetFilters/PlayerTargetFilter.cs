using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarlosLab.UtilityIntelligence;

public class PlayerTargetFilter : TargetFilter
{
    protected override bool OnFilterTarget(UtilityEntity target)
    {
        return target.EntityFacade is PlayerEntityFacade;
    }
}
