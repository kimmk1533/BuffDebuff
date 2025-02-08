using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public sealed class ProjectileAnimator : BaseAnimator, IAnim_Death
	{
		#region 변수
		private Projectile m_Projectile = null;

		private float m_DeathAnimationDelay = 0f;
		#endregion

		#region 프로퍼티
		public float deathAnimationDelay
		{
			get
			{
				if (m_Animator == null ||
					m_Animator.runtimeAnimatorController == null)
					return 0f;

				if (m_DeathAnimationDelay > 0f)
					return m_DeathAnimationDelay;

				var tempClips = m_Animator.runtimeAnimatorController.animationClips;
				foreach (var clip in tempClips)
				{
					if (clip.name.Contains("Death") == true)
					{
						return m_DeathAnimationDelay = clip.length;
					}
				}

				if (m_DeathAnimationDelay <= 0f)
					Debug.LogError("Death 애니메이션 없음");

				return m_DeathAnimationDelay;
			}
		}
		#endregion

		#region 매니져
		private static ProjectileManager M_Projectile => ProjectileManager.Instance;
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			this.NullCheckGetComponentInParent<Projectile>(ref m_Projectile);
		}
		public override void Finallize()
		{
			base.Finallize();

			m_SpriteRenderer.flipX = false;
			m_SpriteRenderer.flipY = false;
		}

		public void FlipX(bool flip)
		{
			m_SpriteRenderer.flipX = flip;
		}
		public void FlipY(bool flip)
		{
			m_SpriteRenderer.flipY = flip;
		}

		public void Anim_Death()
		{
			if (m_Animator == null ||
				m_Animator.runtimeAnimatorController == null)
				M_Projectile.Despawn(m_Projectile);

			m_Animator.SetTrigger("Death");
		}

		// Anim Event
		private void AnimEvent_Despawn()
		{
			M_Projectile.Despawn(m_Projectile);
		}
	}
}