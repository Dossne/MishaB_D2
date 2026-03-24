using System;
using System.Collections;
using System.Collections.Generic;
using VacuumSorter.Items;
using VacuumSorter.LevelFlow;
using UnityEngine;

namespace VacuumSorter.SortTargets
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class SortTargetView : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly HashSet<ItemView> _processingItems = new();

        private ItemTypeConfig _acceptedType;
        private float _pullDuration;
        private float _sinkDepth;
        private float _wrongRejectImpulse;
        private Action<ItemTypeConfig> _onAccepted;
        private MaterialPropertyBlock _propertyBlock;
        private Transform _sinkPoint;

        public void Initialize(
            ItemTypeConfig acceptedType,
            LevelConfig.TargetInteractionSettings interactionSettings,
            Action<ItemTypeConfig> onAccepted)
        {
            _acceptedType = acceptedType;
            _pullDuration = interactionSettings != null ? interactionSettings.PullDuration : 0.24f;
            _sinkDepth = interactionSettings != null ? interactionSettings.SinkDepth : 0.55f;
            _wrongRejectImpulse = interactionSettings != null ? interactionSettings.WrongItemRejectImpulse : 1.6f;
            _onAccepted = onAccepted;

            var acceptRadius = interactionSettings != null ? interactionSettings.AcceptRadius : 0.85f;
            var acceptHeight = interactionSettings != null ? interactionSettings.AcceptHeight : 0.8f;

            ConfigureTrigger(acceptRadius, acceptHeight);
            EnsureVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_acceptedType == null)
            {
                return;
            }

            var itemView = other.GetComponentInParent<ItemView>();
            if (itemView == null || _processingItems.Contains(itemView))
            {
                return;
            }

            if (itemView.ItemType != _acceptedType)
            {
                RejectWrongItem(other.attachedRigidbody);
                return;
            }

            StartCoroutine(AcceptItemRoutine(itemView));
        }

        private void ConfigureTrigger(float radius, float height)
        {
            var trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, height * 0.5f, 0f);
            trigger.size = new Vector3(radius * 2f, height, radius * 2f);
        }

        private void EnsureVisuals()
        {
            if (_acceptedType == null)
            {
                return;
            }

            var ring = EnsurePrimitive(
                "TargetRing",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.04f, 0f),
                new Vector3(1.7f, 0.04f, 1.7f));
            ApplyColor(ring, _acceptedType.BaseColor);

            var inner = EnsurePrimitive(
                "TargetInner",
                PrimitiveType.Cylinder,
                new Vector3(0f, 0.01f, 0f),
                new Vector3(1.1f, 0.02f, 1.1f));
            var innerColor = Color.Lerp(_acceptedType.BaseColor, Color.black, 0.48f);
            ApplyColor(inner, innerColor);

            var marker = EnsurePrimitive(
                "TargetTypeMarker",
                _acceptedType.PrimitiveType,
                new Vector3(0f, 0.35f, 0f),
                _acceptedType.VisualScale * 0.55f);
            ApplyColor(marker, _acceptedType.BaseColor);

            _sinkPoint = EnsureSinkPoint();
        }

        private Transform EnsureSinkPoint()
        {
            var sink = transform.Find("SinkPoint");
            if (sink == null)
            {
                sink = new GameObject("SinkPoint").transform;
                sink.SetParent(transform, false);
            }

            sink.localPosition = new Vector3(0f, -_sinkDepth, 0f);
            sink.localRotation = Quaternion.identity;
            sink.localScale = Vector3.one;
            return sink;
        }

        private void RejectWrongItem(Rigidbody body)
        {
            if (body == null || _wrongRejectImpulse <= 0f)
            {
                return;
            }

            var away = body.position - transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.001f)
            {
                away = transform.right;
            }

            var impulse = away.normalized + Vector3.up * 0.2f;
            body.AddForce(impulse * _wrongRejectImpulse, ForceMode.VelocityChange);
        }

        private IEnumerator AcceptItemRoutine(ItemView itemView)
        {
            _processingItems.Add(itemView);

            var itemTransform = itemView.transform;
            var startPosition = itemTransform.position;
            var startScale = itemTransform.localScale;
            var targetPosition = _sinkPoint != null ? _sinkPoint.position : transform.position + Vector3.down * _sinkDepth;

            var body = itemView.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.useGravity = false;
                body.isKinematic = true;
            }

            var colliders = itemView.GetComponents<Collider>();
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            var elapsed = 0f;
            while (elapsed < _pullDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / _pullDuration);
                itemTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
                itemTransform.localScale = Vector3.Lerp(startScale, startScale * 0.2f, t);
                yield return null;
            }

            _onAccepted?.Invoke(_acceptedType);
            Destroy(itemView.gameObject);
        }

        private GameObject EnsurePrimitive(string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale)
        {
            var existing = transform.Find(name);
            GameObject result;
            if (existing == null)
            {
                result = GameObject.CreatePrimitive(primitiveType);
                result.name = name;
                result.transform.SetParent(transform, false);
            }
            else
            {
                result = existing.gameObject;
            }

            result.transform.localPosition = localPosition;
            result.transform.localRotation = Quaternion.identity;
            result.transform.localScale = localScale;

            var collider = result.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            return result;
        }

        private void ApplyColor(GameObject gameObject, Color color)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, color);
            _propertyBlock.SetColor(ColorId, color);
            renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
