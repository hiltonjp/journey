﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

using Storm.Characters.Player;
using Storm.Services;

namespace Tests {
  public class RunningTests : StateTest<Running> {

    [Test]
    public void Running_Can_WallRun() {
      SetupTest();

      player.PressedJump().Returns(true);
      player.IsTouchingLeftWall().Returns(true);

      state.OnUpdate();
    
      AssertStateChange<WallRun>();
    }

    [Test]
    public void Running_Can_StartJump() {
      SetupTest();

      player.PressedJump().Returns(true);
      player.IsTouchingLeftWall().Returns(false);
      player.IsTouchingRightWall().Returns(false);

      state.OnUpdate();
    
      AssertStateChange<SingleJumpStart>();
    }

    [Test]
    public void Running_Can_Dive() {
      SetupTest();
    
      player.PressedJump().Returns(false);
      player.HoldingDown().Returns(true);

      state.OnUpdate();

      AssertStateChange<Dive>();
    }
    
    [Test]
    public void Running_Can_SlowToIdle() {
      SetupTest();
    
      settings.IdleThreshold = 0.5f;
      state.OnStateAdded();

      physics.Velocity = Vector2.zero;

      player.GetHorizontalInput().Returns(0);
      player.CanMove().Returns(true);
      player.IsTouchingGround().Returns(true);

      state.OnFixedUpdate();

      AssertStateChange<Idle>();
    }


    [Test]
    public void Running_Can_Fall() {
      SetupTest();

      settings.IdleThreshold = 0.5f;
      settings.Deceleration = 1;
      state.OnStateAdded();

      physics.Velocity = new Vector2(1, 0);

      player.GetHorizontalInput().Returns(1);
      player.CanMove().Returns(true);
      player.IsTouchingGround().Returns(false);
      player.IsFalling().Returns(true);

      state.OnFixedUpdate();

      AssertStateChange<SingleJumpFall>();
    }

    [Test]
    public void Running_Starts_CoyoteTime() {
      SetupTest();

      settings.IdleThreshold = 0.5f;
      settings.Deceleration = 1;
      state.OnStateAdded();

      physics.Velocity = new Vector2(1, 0);

      player.GetHorizontalInput().Returns(1);
      player.CanMove().Returns(true);
      player.IsTouchingGround().Returns(false);
      player.IsFalling().Returns(true);

      state.OnFixedUpdate();

      player.Received().StartCoyoteTime();
    }

    [Test]
    public void Running_FixedUpdate_NoChange() {
      SetupTest();
      
      settings.IdleThreshold = 0.5f;
      settings.Deceleration = 1;
      state.OnStateAdded();

      physics.Velocity = new Vector2(1, 0);

      player.GetHorizontalInput().Returns(1);
      player.CanMove().Returns(true);
      player.IsTouchingGround().Returns(true);

      state.OnFixedUpdate();
      
      AssertNoStateChange<Idle>();
      AssertNoStateChange<SingleJumpFall>();
    }

    [Test]
    public void Running_Update_NoChange() {
      SetupTest();
      
      player.PressedJump().Returns(false);
      player.HoldingDown().Returns(false);

      state.OnUpdate();
      
      AssertNoStateChange<WallRun>();
      AssertNoStateChange<SingleJumpStart>();
      AssertNoStateChange<Dive>();
    }
  }
}