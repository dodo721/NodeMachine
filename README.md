# NodeMachine

A custom editor tool for Unity. Provides a node editor to give state machine like control of entities,
which are linked to custom written C# functions.

Uses IML assembly generation with reflection to expose private variables at runtime, enabling inspection and editing
of any tagged variable attached to a NodeMachine.

Shows realtime state of flow, and contains error handling with error tracing through the node graph.
