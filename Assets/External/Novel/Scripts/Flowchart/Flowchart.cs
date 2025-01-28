using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;
using System.Linq;

namespace Novel
{
    [Serializable]
    public class Flowchart
    {
        public enum StopType
        {
            [InspectorName("���̃t���[�`���[�g�̂�")] Single,
            [InspectorName("await���̐e���܂�")] IncludeParent,
        }

        // �V���A���C�Y����
        [SerializeField]
        List<CommandData> commandDataList = new();

        /// <summary>
        /// ���݌Ăяo����Ă��邩
        /// </summary>
        bool isCalling;

        /// <summary>
        /// �Ăяo���Ă���R�}���h�̃C���f�b�N�X
        /// </summary>
        int callIndex;

        /// <summary>
        /// ��~������ۂ̃t���O
        /// </summary>
        bool isSingleStopped;

        public FlowchartCallStatus CallStatus { get; set; }
        public IReadOnlyList<CommandData> GetReadOnlyCommandDataList() => commandDataList;

        /// <summary>
        /// �t���[�`���[�g���Ăяo���܂�
        /// </summary>
        /// <param name="index">���X�g�̉��Ԗڂ��甭�΂��邩</param>
        /// <param name="token">�L�����Z���p�̃g�[�N��</param>
        public UniTask ExecuteAsync(int index = 0, CancellationToken token = default)
        {
            SetStatus(token);
            return PrecessAsync(index);
        }

        // �ʏ�A������̓��[�U�[����Ăяo���܂���
        public UniTask ExecuteAsync(int index, FlowchartCallStatus callStatus)
        {
            SetStatus(callStatus);
            return PrecessAsync(index);
        }

        async UniTask PrecessAsync(int index)
        {
            isCalling = true;
            SetIndex(index, false);

            while (commandDataList.Count > callIndex && isSingleStopped == false)
            {
                var cmdData = commandDataList[callIndex];
                await cmdData.ExecuteAsync(this);
                callIndex++;
            }

            if (CallStatus.ExistWaitOthers == false && isSingleStopped == false)
            {
                NovelManager.Instance.ClearAllUI();
            }
            isSingleStopped = false;
            isCalling = false;
            callIndex = 0;
        }

        /// <summary>
        /// �t���[�`���[�g���~���܂�
        /// </summary>
        public void Stop(StopType stopType = StopType.IncludeParent, bool isClearUI = false)
        {
            if (stopType == StopType.IncludeParent)
            {
                CallStatus.Cts?.Cancel();
                if (isCalling && isClearUI)
                {
                    NovelManager.Instance.ClearAllUI();
                }
            }
            else if (stopType == StopType.Single)
            {
                isSingleStopped = true;
            }
        }

        // �R�}���h���ŌĂ΂ꂽ�ۂ͔�����ۂ�index��+1����̂ŁA���̕��\�߈����Ă���
        public void SetIndex(int index, bool calledInCommand)
        {
            if (calledInCommand)
            {
                callIndex = index - 1;
            }
            else
            {
                callIndex = index;
            }
        }

        void SetStatus(CancellationToken token)
        {
            CancellationTokenSource cts = new();
            if (token != default)
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(token, cts.Token);
            }
            CallStatus = new(cts, false);
        }
        void SetStatus(FlowchartCallStatus status)
        {
            CallStatus = status;
        }

#if UNITY_EDITOR
        [SerializeField, TextArea, Tooltip("�G�f�B�^�ł̂ݎg�p����܂�")]
        string description;
        public string Description => description;

        public List<CommandData> GetCommandDataList() => commandDataList;
        public void SetCommandDataList(IEnumerable<CommandData> commands)
        {
            commandDataList = commands.ToList();
            for (int i = 0; i < commandDataList.Count; i++)
            {
                var cmd = commandDataList[i].GetCommandBase();
                if (cmd == null) continue;
                cmd.SetFlowchart(this);
                cmd.SetIndex(i);
            }
        }

        public bool EqualsCommands(Flowchart other)
        {
            if (other == null) return false;
            var myList = GetReadOnlyCommandDataList();
            var otherList = other.GetReadOnlyCommandDataList();
            if (myList == null && otherList == null) return true;
            if (myList == null || otherList == null || myList.Count != otherList.Count) return false;

            bool isEqual = true;
            for (int i = 0; i < myList.Count; i++)
            {
                if (myList[i] != otherList[i])
                {
                    isEqual = false;
                    break;
                }
            }
            return isEqual;
        }
#endif
    }

    /// <summary>
    /// Token, TokenSource��"���̃t���[�`���[�g���I����ҋ@���Ă��邩"��3�̏���ێ����܂�
    /// </summary>
    public class FlowchartCallStatus
    {
        public readonly CancellationTokenSource Cts;
        public readonly CancellationToken Token;

        /// <summary>
        /// �ҋ@���Ă���ʂ̃t���[�`���[�g�����݂��邩
        /// </summary>
        public readonly bool ExistWaitOthers;

        public FlowchartCallStatus(CancellationTokenSource cts, bool existWaitOthers)
        {
            Token = cts.Token;
            Cts = cts;
            ExistWaitOthers = existWaitOthers;
        }
    }
}