﻿using AutoBogus.Playground.Model;

namespace AutoBogus.Playground;

public sealed class ItemFaker
    : AutoFaker<Item>
{
    public ItemFaker(Guid id)
    {
        RuleFor(item => item.Id,     () => id);
        RuleFor(item => item.Status, () => ItemStatus.Pending);
    }
}
