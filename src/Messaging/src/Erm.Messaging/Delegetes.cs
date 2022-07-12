using System;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public delegate T Factory<out T>(IServiceProvider serviceProvider);