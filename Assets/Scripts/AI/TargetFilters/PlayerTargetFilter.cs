using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarlosLab.UtilityIntelligence;

public class PlayerTargetFilter : TargetFilter
{
    protected override bool OnFilterTarget(UtilityEntity target)
    {
        if (target == null) return false;
        if (target.EntityFacade == null) return false;
        return target.EntityFacade is PlayerEntityFacade;
    }
}
