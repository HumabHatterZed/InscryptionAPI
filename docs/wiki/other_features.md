## Other Features
Listed here are various miscellaneous features that the API adds that aren't covered in other articles, or aren't covered in much detail.

## Negative and Self-Damage
With the API, it is now possible to make cards deal damage to their owner.
This is done by reducing the value of CombatPhaseManager.DamageDealtThisPhase so it becomes negative.

## Vanilla Shield Stacking
With the introduction of the ShieldManager, the Nano Armour (Armoured) sigil can now stack, providing multiple shields.

To minimise inteference with the vanilla game, some interactions have been altered as well;
certain vanilla items and sigils that reset shields will now add an additional shield instead, provided a given card doesn't already have a shield.