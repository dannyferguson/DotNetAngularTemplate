﻿namespace PlaywrightTests.Collections;

using Xunit;
using Fixtures;

[CollectionDefinition("Global Test Setup", DisableParallelization = true)]
public class GlobalTestCollection : 
    ICollectionFixture<GlobalFixture>, 
    ICollectionFixture<DatabaseFixture>
{
    // Empty on purpose.
}
