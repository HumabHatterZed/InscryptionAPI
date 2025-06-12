## Triggers and Order of Operations
This article is for modders looking for specific information on the order triggers are called in various contexts.
The information presented here is fairly technical and dry, and assumes you have an understanding of how Inscryption's code works.

Triggers are listed in the order they are called, with the first entry being the first trigger called and the last entry being the last trigger called.

Custom triggers start with the letter 'I' and are implemented via interfaces, and the rest are vanilla triggers.

## Card Damage
Handled by `PlayableCard.TakeDamage`, `ShieldManager.TriggerBreakShield`, and `ShieldManager.BreakShield`.

Cards with shields only call the first four triggers listed.

|Trigger|Additional information|
|:-|:-|
|IModifyDamageTaken|Sets the damage amount to the returned value|
|IPreTakeDamage|Called before shields are checked|
|IShieldPreventedDamage|Only called when a card has a shield|
|IShieldPreventedDamageInHand|Only called when a card has a shield <br><br> Called on cards in the player's hand|
|OnTakeDamage|Called for cards without shields|
|OtherCardDealtDamage|Called after `PlayableCard.Die`|
|IOnOtherCardDealtDamageInHand|Called on card's in the player's hand|

## Card Death
Handled by `PlayableCard.Die`.

|Trigger|Additional information|
|:-|:-|
|OnPreDeathAnimation||
|OnOtherCardPreDeath||
|IOnOtherCardPreDeathInHand|Called on cards in the player's hand|
|OnOtherCardDie||
|IOnOtherCardDieInHand|Called on cards in the player's hand|

## Card Drawing
Handled by `PlayerHand.AddCardToHand`.

|Trigger|Additional information|
|:-|:-|
|OnDrawn|Called before a card is added to `cardsInHand`|
|OnOtherCardDrawn|Called before a card is added to `cardsInHand`|
|IOnAddedToHand|Called after a card is added to `cardsInHand`|
|IOtherCardAddedToHand|Called after a card is added to `cardsInHand`|

## Card Playing
Handled by `BoardManager.TransitionAndResolveCreatedCard`, `BoardManager.ResolveCardOnBoard`, and `BoardManager.AssignCardToSlot`.

|Trigger|Additional information|
|:-|:-|
|OnOtherCardAssignedToSlot||
|IOnCardAssignedToSlotContext|Provides information on new and old slot|
|IOnOtherCardAssignedToSlotInHand|Called on cards in the player's hand|
|IOnCardAssignedToSlotNoResolve|Only called if assigned card was already resolved|
|OnResolveOnBoard|Called when a card is played from the hand or queue|
|OnOtherCardResolve|Called when a card is played from the hand or queue|
|IOnOtherCardResolveInHand|Called on cards in the player's hand <br><br> Called when a card is played from the hand or queue|

## Combat Phase
Handled by `CombatPhaseManager.DoCombatPhase`, `CombatPhaseManager.SlotAttackSequence`, and `CombatPhaseManager.SlotAttackSlot`.

`SlotAttackSequence` is called for every attacking card, then `SlotAttackSlot` is called for every time a card attacks.

|Trigger|Additional information|
|:-|:-|
|IOnBellRun|Called before `DoCombatPhase`|
|IOnPreSlotAttackSequence|Called before `SlotAttackSequence`|
|IGetAttackingSlots|Adds return value to current list of attacking slots, can modify current list within the method|
|ISetupAttackSequence|Replaces current list of targeted slots with return value<br><br>Called with priority `ReplacesDefaultOpposingSlot`|
|IGetOpposingSlots|Adds return value to current list of targeted slots, can modify current list within the method|
|ISetupAttackSequence|Replaces current list of targeted slots with return value<br><br>Called with priority `Normal`|
|ISetupAttackSequence|Replaces current list of targeted slots with return value<br><br>Called with priority `BringsBackOpposingSlot`|
|ISetupAttackSequence|Replaces current list of targeted slots with return value<br><br>Called with priority `PostAdditionModification`|
|OnSlotTargetedForAttack||
|IModifyDirectDamage|Called only if direct damage is being dealt<br><br>Sets the damage being dealt to the returned value|
|OnDealDamageDirectly||
|IOnCardDealtDamageDirectly|Called for every card on the board|
|OnCardGettingAttacked|Called only if a card is being attacked|
|IPostCardGettingAttacked||
|IOnPostSingularSlotAttackSlot|Called after `SlotAttackSlot`|
|OnAttackEnded|Called after all cards have finished attacking|
|IOnPostSlotAttackSequence|Called after `SlotAttackSequence`|

## Consumable Items
Handled by `ConsumableItemSlot.ConsumeItem` and each ConsumableItem's `ActivateSequence`.

|Trigger|Additional information|
|:-|:-|
|IItemCanBeUsed|Can stop items from being used|
|IOnItemPreventedFromUse|Called if `IItemCanBeUsed` stopped an item from being used|
|IOnPreItemUsed|Called before `ConsumeItem` and `ActivateSequence`|
|IOnPostItemUsed|Called after `ConsumeItem` and `ActivateSequence`|

## Passive Stat Buffs
Handled by `PlayableCard.GetPassiveAttackBuffs` and `PlayableCard.GetPassiveHealthBuffs`.

These patches are called every frame.

|Trigger|Additional information|
|:-|:-|
|IOnCardPassiveAttackBuffs|Sets attack buff to returned value|
|IPassiveAttackBuffs|Adds to attack buff with returned value|

|Trigger|Additional information|
|:-|:-|
|IOnCardPassiveHealthBuffs|Sets health buff to returned value|
|IPassiveHealthBuffs|Adds to health buff with returned value|

## Scale Damage
Handled by `LifeManager.ShowDamageSequence`.

|Trigger|Additional information|
|:-|:-|
|IOnPreScalesChangedRef|Modifies the damage being dealt and weights being added to the physical scale|
|IOnPreScalesChanged|Called before `ShowDamageSequence`|
|IOnPostScalesChanged|Called after `ShowDamageSequence`|

## Turn Sequence
Handled by `TurnManager.GameSequence`, `TurnManager.PlayerTurn`, `TurnManager.OpponentTurn`, `TurnManager.DoUpkeepPhase`.

Triggers for the opponent's turn are not called if their turn is skipped.

|Trigger|Additional information|
|:-|:-|
|OnUpkeep||
|IOnUpkeepInHand|Called on cards in the player's hand|
|OnTurnEnd|Called after `DoCombatPhase`|
|IOnTurnEndInQueue|Called on cards in the opponent's queue|
|IOnTurnEndInHand|Called on cards in the player's hand|
