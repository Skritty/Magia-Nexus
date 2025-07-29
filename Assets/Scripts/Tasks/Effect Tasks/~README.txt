|Effect Tasks|
Effect Tasks do an effect from one Entity (the owner) to another Entity (the target).
They take in as input: Entity owner, Entity target, float multiplier, and bool triggered.
Effect Tasks determine their targets using a targeting method field (of the Targeting class).
They can recieve any data type (and as such will work in any list of Tasks regardless of type), but will only use data of type Effect.
Effect data will multiply its effect multiplier down into the effect task.
Effect multiplier does not need to be used, but it can be.
All Effect Tasks should be completely generic.