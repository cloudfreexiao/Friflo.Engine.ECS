﻿using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Index {


public static class Test_Index_Delete
{
    [Test]
    public static void Test_Index_Delete_Value()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        var values  = store.ComponentIndex<IndexedInt, int>().Values;
        
        entity1.AddComponent(new IndexedInt { value = 10 });
        entity2.AddComponent(new IndexedInt { value = 10 });
        entity3.AddComponent(new IndexedInt { value = 20 });
        AreEqual("{ 10, 20 }",  values.Debug());
        
        entity1.DeleteEntity();
        AreEqual("{ 10, 20 }",  values.Debug());
        
        entity2.DeleteEntity();
        AreEqual("{ 20 }",      values.Debug());
        
        entity3.DeleteEntity();
        AreEqual("{ }",         values.Debug());
    }
    
    [Test]
    public static void Test_Index_Delete_Class()
    {
        var store   = new EntityStore();
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        var values  = store.ComponentIndex<IndexedName, string>().Values;
        
        entity1.AddComponent(new IndexedName { name = "abc" });
        entity2.AddComponent(new IndexedName { name = "abc" });
        entity3.AddComponent(new IndexedName { name = "xyz" });
        AreEqual("{ abc, xyz }",    values.Debug());
        
        entity1.DeleteEntity();
        AreEqual("{ abc, xyz }",    values.Debug());
        
        entity2.DeleteEntity();
        AreEqual("{ xyz }",         values.Debug());
        
        entity3.DeleteEntity();
        AreEqual("{ }",             values.Debug());
    }
    
    [Test]
    public static void Test_Index_Delete_Entity()
    {
        var store   = new EntityStore();
        
        var entity1  = store.CreateEntity(1);
        var entity2  = store.CreateEntity(2);
        var entity3  = store.CreateEntity(3);
        
        var entity10 = store.CreateEntity(10);
        var entity11 = store.CreateEntity(11);
        
        var values  = store.ComponentIndex<AttackComponent, Entity>().Values;
        
        entity1.AddComponent(new AttackComponent { target = entity10 });                    //  1  🡒  10     2
        AreEqual("{ 1 }",       entity10.GetIncomingLinks<AttackComponent>().Debug());      //  3      11
                                                                                    
        entity2.AddComponent(new AttackComponent { target = entity10 });                    //  1  🡒  10  🡐  2
        AreEqual("{ 1, 2 }",    entity10.GetIncomingLinks<AttackComponent>().Debug());      //  3      11
                                                                                    
        entity3.AddComponent(new AttackComponent { target = entity11 });                    //  1  🡒  10  🡐  2
        AreEqual("{ 3 }",       entity11.GetIncomingLinks<AttackComponent>().Debug());      //  3  🡒  11
        AreEqual("{ 10, 11 }",  values.Debug());
        
        entity1.DeleteEntity();                                                             //  -     10  🡐  2
        AreEqual("{ 2 }",       entity10.GetIncomingLinks<AttackComponent>().Debug());      //  3  🡒  11
        AreEqual("{ 10, 11 }",  values.Debug());                                    
        
        entity2.DeleteEntity();                                                             //  -     10     -
        AreEqual("{ }",         entity10.GetIncomingLinks<AttackComponent>().Debug());      //  3  🡒  11
        AreEqual("{ 11 }",      values.Debug());                                    
        
        entity3.DeleteEntity();                                                             //  -     10     -
        AreEqual("{ }",         entity11.GetIncomingLinks<AttackComponent>().Debug());      //  -     11
        AreEqual("{ }",         values.Debug());                                    
    }
    
    [Test]
    public static void Test_Index_Delete_linked_entities()
    {
        var store   = new EntityStore();
        
        var entity1  = store.CreateEntity(1);
        var entity2  = store.CreateEntity(2);
        var entity3  = store.CreateEntity(3);
        
        var target10 = store.CreateEntity(10);
        var target11 = store.CreateEntity(11);
        
        var targets = store.ComponentIndex<AttackComponent, Entity>().Values;
        
        entity1.AddComponent(new AttackComponent { target = target10 });
        entity2.AddComponent(new AttackComponent { target = target10 });
        entity3.AddComponent(new AttackComponent { target = target11 });                    //  1  🡒  10  🡐  2
        // --- initial targets state                                                        //  3  🡒  11
        AreEqual("{ 1, 2 }",    target10.GetIncomingLinks<AttackComponent>().Debug());
        AreEqual("{ 3 }",       target11.GetIncomingLinks<AttackComponent>().Debug());
        AreEqual("{ 10, 11 }",  targets.Debug());

        target10.DeleteEntity();                                                            //  1      -      2
        IsFalse (               entity1.HasComponent<AttackComponent>());                   //  3  🡒  11
        IsFalse (               entity2.HasComponent<AttackComponent>());
        AreEqual("{ 11 }",      targets.Debug());
        
        target11.DeleteEntity();                                                            //  1      -      2
        IsFalse (               entity3.HasComponent<AttackComponent>());                   //  3      -
        AreEqual("{ }",         targets.Debug());
    }
    
    [Test]
    public static void Test_Index_Delete_self_referencing_entity()
    {
        var store   = new EntityStore();
        
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
       
        var targets = store.ComponentIndex<AttackComponent, Entity>().Values;
        
        entity1.AddComponent(new AttackComponent { target = entity1 });
        entity2.AddComponent(new AttackComponent { target = entity1 });
        entity3.AddComponent(new AttackComponent { target = entity1 });                 //  2  🡒  1  🡐  3
                                                                                        //        ⮍
        // --- initial targets state
        AreEqual("{ 1, 2, 3 }", entity1.GetIncomingLinks<AttackComponent>().Debug());
        AreEqual("{ 1 }",       targets.Debug());
        
        entity1.DeleteEntity();                                                         //  2     -      3
        IsFalse (               entity2.HasComponent<AttackComponent>());               //
        IsFalse (               entity3.HasComponent<AttackComponent>());
        AreEqual("{ }",         targets.Debug());
    }
    
    [Test]
    public static void Test_Index_Delete_Range()
    {
        var store   = new EntityStore();
        
        var entity1 = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        
        
        var values  = store.ComponentIndex<IndexedIntRange, int>().Values;
        
        entity1.AddComponent(new IndexedIntRange { value = 10 });
        entity2.AddComponent(new IndexedIntRange { value = 10 });
        entity3.AddComponent(new IndexedIntRange { value = 20 });
        AreEqual("{ 10, 20 }",  values.Debug());
        
        entity1.DeleteEntity();
        AreEqual("{ 10, 20 }",  values.Debug());
        
        entity2.DeleteEntity();
        AreEqual("{ 20 }",      values.Debug());
        
        entity3.DeleteEntity();
        AreEqual("{ }",         values.Debug());
    }
    
    [Test]
    public static void Test_Index_DeleteEntity_Entity() {
        var store   = new EntityStore();
        var root    = store.CreateEntity(1);
        var entity2 = store.CreateEntity(2);
        var entity3 = store.CreateEntity(3);
        var child   = store.CreateEntity(new AttackComponent { target = entity2 });
        child.GetComponent<AttackComponent>().target = entity3;   // causes subsequent exception. The indexed value MUST never be changed
        root.AddChild(child);
        var e = Throws<InvalidOperationException>(() => {
            child.DeleteEntity();    
        });
        AreEqual("Indexed value of 'AttackComponent' not found. Reason: indexed values MUST NOT be changed. See: https://friflo.gitbook.io/friflo.engine.ecs/documentation/component-index#indexed-components", e!.Message);
    }
    
    [Test]
    public static void Test_Index_DeleteEntity_struct() {
        var store   = new EntityStore();
        var entity  = store.CreateEntity(new IndexedInt { value = 1 });
        entity.GetComponent<IndexedInt>().value = 2;   // causes subsequent exception. The indexed value MUST never be changed

        var e = Throws<InvalidOperationException>(() => {
            entity.DeleteEntity();    
        });
        AreEqual("Indexed value of 'IndexedInt' not found. Reason: indexed values MUST NOT be changed. See: https://friflo.gitbook.io/friflo.engine.ecs/documentation/component-index#indexed-components", e!.Message);
    }
    
    [Test]
    public static void Test_Index_DeleteEntity_class() {
        var store   = new EntityStore();
        var entity  = store.CreateEntity(new IndexedName { name = "abc" });
        entity.GetComponent<IndexedName>().name = "xyz";   // causes subsequent exception. The indexed value MUST never be changed
        var e = Throws<InvalidOperationException>(() => {
            entity.DeleteEntity();    
        });
        AreEqual("Indexed value of 'IndexedName' not found. Reason: indexed values MUST NOT be changed. See: https://friflo.gitbook.io/friflo.engine.ecs/documentation/component-index#indexed-components", e!.Message);
    }
}

}
