﻿/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.Hit;
using TouchScript.Utils;
using UnityEngine;

namespace TouchScript.Layers
{
    [AddComponentMenu("TouchScript/Layers/Fullscreen Layer")]
    public sealed class FullscreenLayer : TouchLayer
    {

        #region Constants

        public enum LayerType
        {
            MainCamera,
            Camera,
            Global
        }

        #endregion

        #region Public properties

        public LayerType Type
        {
            get { return type; }
            set
            {
                if (value == type) return;
                type = value;
                updateCamera();
                cacheCameraTransform();
            }
        }

        public Camera Camera
        {
            get { return _camera; }
            set
            {
                if (value == _camera) return;
                _camera = value;
                if (_camera == null) Type = LayerType.Global;
                else Type = LayerType.Camera;
            }
        }

        /// <inheritdoc />
        public override Vector3 WorldProjectionNormal
        {
            get
            {
                if (cameraTransform == null) return transform.forward;
                return cameraTransform.forward;
            }
        }

        #endregion

        #region Private variables

        [SerializeField]
        private LayerType type = LayerType.MainCamera;
        [SerializeField]
        private Camera _camera;

        private Transform cameraTransform;

        #endregion

        #region Public methods

        /// <inheritdoc />
        public override LayerHitResult Hit(Vector2 position, out ITouchHit hit)
        {
            if (base.Hit(position, out hit) == LayerHitResult.Miss) return LayerHitResult.Miss;

            if (_camera != null)
            {
                if (!_camera.pixelRect.Contains(position)) return LayerHitResult.Miss;
            }

            hit = TouchHitFactory.Instance.GetTouchHit(transform);
            var hitTests = transform.GetComponents<HitTest>();
            if (hitTests.Length == 0) return LayerHitResult.Hit;

            foreach (var test in hitTests)
            {
                if (!test.enabled) continue;
                var hitResult = test.IsHit(hit);
                if (hitResult == HitTest.ObjectHitResult.Miss || hitResult == HitTest.ObjectHitResult.Discard) return LayerHitResult.Miss;
            }

            return LayerHitResult.Hit;
        }

        public override Vector3 ProjectTo(Vector2 screenPosition, Plane projectionPlane)
        {
            if (_camera == null) return base.ProjectTo(screenPosition, projectionPlane);
            else return ProjectionUtils.CameraToPlaneProjection(screenPosition, _camera, projectionPlane);
        }

        #endregion

        #region Unity methods

        protected override void Awake()
        {
            updateCamera();
            cacheCameraTransform();

            base.Awake();
        }

        // To be able to turn it off
        private void OnEnable()
        {}

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override void setName()
        {
            if (_camera == null || _camera == Camera.main) Name = "Global Fullscreen";
            else Name = "Fullscreen @ " + _camera.name;
        }

        #endregion

        #region Private functions

        private void updateCamera()
        {
            switch (type)
            {
                case LayerType.Global:
                    _camera = null;
                    break;
                case LayerType.MainCamera:
                    _camera = Camera.main;
                    if (_camera == null) Debug.LogError("No Main camera found!");
                    break;
            }
        }

        private void cacheCameraTransform()
        {
            if (_camera == null) cameraTransform = null;
            else cameraTransform = _camera.transform;
        }

        #endregion
    }
}
