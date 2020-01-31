using System;
using System.Collections.Generic;
using System.Linq;
using Assets.SimpleLocalization;
using Libplanet;
using Nekoyume.Action;
using Nekoyume.Model.Mail;
using Nekoyume.Manager;
using Nekoyume.State;
using Nekoyume.TableData;
using Nekoyume.UI;
using UniRx;
using Combination = Nekoyume.Action.Combination;
using Nekoyume.Model.State;

namespace Nekoyume.BlockChain
{
    /// <summary>
    /// 현상태 : 각 액션의 랜더 단계에서 즉시 게임 정보에 반영시킴. 아바타를 선택하지 않은 상태에서 이전에 성공시키지 못한 액션을 재수행하고
    ///       이를 핸들링하면, 즉시 게임 정보에 반영시길 수 없기 때문에 에러가 발생함.
    /// 참고 : 이후 언랜더 처리를 고려한 해법이 필요함.
    /// 해법 1: 랜더 단계에서 얻는 `eval` 자체 혹은 변경점을 queue에 넣고, 게임의 상태에 따라 꺼내 쓰도록.
    /// </summary>
    public class ActionRenderHandler : ActionHandler
    {
        private static class Singleton
        {
            internal static readonly ActionRenderHandler Value = new ActionRenderHandler();
        }

        public static ActionRenderHandler Instance => Singleton.Value;

        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private ActionRenderHandler() : base()
        {
        }

        public void Start()
        {
            Shop();
            Ranking();
            RewardGold();
            CreateAvatar();
            DeleteAvatar();
            HackAndSlash();
            Combination();
            Sell();
            SellCancellation();
            Buy();
            RankingReward();
            AddItem();
            AddGold();
            DailyReward();
            ItemEnhancement();
            QuestReward();
            RankingBattle();
            WeeklyArenaReward();
        }

        public void Stop()
        {
            _disposables.DisposeAllAndClear();
        }

        private void Shop()
        {
            ActionBase.EveryRender(ShopState.Address)
                .ObserveOnMainThread()
                .Subscribe(UpdateShopState).AddTo(_disposables);
        }

        private void Ranking()
        {
            ActionBase.EveryRender(RankingState.Address)
                .ObserveOnMainThread()
                .Subscribe(UpdateRankingState).AddTo(_disposables);
        }

        private void RewardGold()
        {
            ActionBase.EveryRender<RewardGold>()
                .Where(ValidateEvaluationForAgentState)
                .ObserveOnMainThread()
                .Subscribe(UpdateAgentState).AddTo(_disposables);
        }

        private void CreateAvatar()
        {
            ActionBase.EveryRender<CreateAvatar>()
                .Where(ValidateEvaluationForAgentState)
                .ObserveOnMainThread()
                .Subscribe(eval =>
                {
                    UpdateAgentState(eval);
                    UpdateAvatarState(eval, eval.Action.index);
                }).AddTo(_disposables);
        }

        private void DeleteAvatar()
        {
            ActionBase.EveryRender<DeleteAvatar>()
                .Where(ValidateEvaluationForAgentState)
                .ObserveOnMainThread()
                .Subscribe(eval =>
                {
                    UpdateAgentState(eval);
                    UpdateAvatarState(eval, eval.Action.index);
                }).AddTo(_disposables);
        }

        private void HackAndSlash()
        {
            ActionBase.EveryRender<HackAndSlash>()
                .Where(ValidateEvaluationForCurrentAvatarState)
                .ObserveOnMainThread()
                .Subscribe(ResponseHackAndSlash).AddTo(_disposables);
        }

        private void Combination()
        {
            ActionBase.EveryRender<Combination>()
                .Where(ValidateEvaluationForCurrentAvatarState)
                .ObserveOnMainThread()
                .Subscribe(ResponseCombination).AddTo(_disposables);
        }

        private void Sell()
        {
            ActionBase.EveryRender<Sell>()
                .Where(ValidateEvaluationForCurrentAvatarState)
                .ObserveOnMainThread()
                .Subscribe(ResponseSell).AddTo(_disposables);
        }

        private void SellCancellation()
        {
            ActionBase.EveryRender<SellCancellation>()
                .Where(ValidateEvaluationForCurrentAvatarState)
                .ObserveOnMainThread()
                .Subscribe(ResponseSellCancellation).AddTo(_disposables);
        }

        private void Buy()
        {
            ActionBase.EveryRender<Buy>()
                .Where(ValidateEvaluationForAgentState)
                .ObserveOnMainThread()
                .Subscribe(ResponseBuy).AddTo(_disposables);
        }

        private void RankingReward()
        {
            ActionBase.EveryRender<RankingReward>()
                .Where(ValidateEvaluationForAgentState)
                .ObserveOnMainThread()
                .Subscribe(UpdateAgentState).AddTo(_disposables);
        }

        private void AddItem()
        {
            ActionBase.EveryRender<AddItem>()
                .Where(ValidateEvaluationForCurrentAvatarState)
                .ObserveOnMainThread()
                .Subscribe(UpdateCurrentAvatarState).AddTo(_disposables);
        }

        private void AddGold()
        {
            ActionBase.EveryRender<AddGold>()
                .Where(ValidateEvaluationForAgentState)
                .ObserveOnMainThread()
                .Subscribe(eval =>
                {
                    UpdateAgentState(eval);
                    UpdateCurrentAvatarState(eval);
                }).AddTo(_disposables);
        }

        private void ItemEnhancement()
        {
            ActionBase.EveryRender<ItemEnhancement>()
                .Where(ValidateEvaluationForAgentState)
                .ObserveOnMainThread()
                .Subscribe(ResponseItemEnhancement).AddTo(_disposables);
        }

        private void DailyReward()
        {
            ActionBase.EveryRender<DailyReward>()
                .Where(ValidateEvaluationForCurrentAvatarState)
                .ObserveOnMainThread()
                .Subscribe(UpdateCurrentAvatarState).AddTo(_disposables);
        }

        private void QuestReward()
        {
            ActionBase.EveryRender<QuestReward>()
                .Where(ValidateEvaluationForCurrentAvatarState)
                .ObserveOnMainThread()
                .Subscribe(ResponseQuestReward).AddTo(_disposables);
        }

        private void RankingBattle()
        {
            ActionBase.EveryRender<RankingBattle>()
                .Where(ValidateEvaluationForCurrentAvatarState)
                .ObserveOnMainThread()
                .Subscribe(ResponseRankingBattle).AddTo(_disposables);
        }

        private void WeeklyArenaReward()
        {
            ActionBase.EveryRender<WeeklyArenaReward>()
                .Where(ValidateEvaluationForAgentState)
                .ObserveOnMainThread()
                .Subscribe(ResponseWeeklyArenaReward).AddTo(_disposables);
        }

        private void ResponseCombination(ActionBase.ActionEvaluation<Combination> eval)
        {
            var agentAddress = eval.InputContext.Signer;
            var avatarAddress = eval.Action.AvatarAddress;
            var result = eval.Action.Result;
            var itemUsable = result.itemUsable;
            
            LocalStateModifier.ModifyAgentGold(agentAddress, result.gold);
            LocalStateModifier.ModifyAvatarActionPoint(avatarAddress, result.actionPoint);
            foreach (var pair in result.materials)
            {
                LocalStateModifier.AddItem(avatarAddress, pair.Key.Data.ItemId, pair.Value);
            }
            LocalStateModifier.RemoveItem(avatarAddress, itemUsable.ItemId);
            LocalStateModifier.AddNewAttachmentMail(avatarAddress, itemUsable.ItemId);
            RenderQuest(avatarAddress, eval.Action.completedQuestIds);

            var format = LocalizationManager.Localize("NOTIFICATION_COMBINATION_COMPLETE");
            UI.Notification.Push(MailType.Workshop, string.Format(format, itemUsable.Data.GetLocalizedName()));
            AnalyticsManager.Instance.OnEvent(AnalyticsManager.EventName.ActionCombinationSuccess);
            UpdateAgentState(eval);
            UpdateCurrentAvatarState(eval);
        }

        private void ResponseSell(ActionBase.ActionEvaluation<Sell> eval)
        {
            var avatarAddress = eval.Action.sellerAvatarAddress;
            var itemId = eval.Action.itemUsable.ItemId;

            LocalStateModifier.AddItem(avatarAddress, itemId);
            var format = LocalizationManager.Localize("NOTIFICATION_SELL_COMPLETE");
            UI.Notification.Push(MailType.Auction, string.Format(format, eval.Action.itemUsable.GetLocalizedName()));
            UpdateCurrentAvatarState(eval);
        }

        private void ResponseSellCancellation(ActionBase.ActionEvaluation<SellCancellation> eval)
        {
            var avatarAddress = eval.Action.sellerAvatarAddress;
            var itemId = eval.Action.result.itemUsable.ItemId;

            LocalStateModifier.RemoveItem(avatarAddress, itemId);
            LocalStateModifier.AddNewAttachmentMail(avatarAddress, itemId);
            var format = LocalizationManager.Localize("NOTIFICATION_SELL_CANCEL_COMPLETE");
            UI.Notification.Push(MailType.Auction, string.Format(format, eval.Action.result.itemUsable.GetLocalizedName()));
            UpdateCurrentAvatarState(eval);
        }

        private void ResponseBuy(ActionBase.ActionEvaluation<Buy> eval)
        {
            var buyerAvatarAddress = eval.Action.buyerAvatarAddress;
            var price = eval.Action.sellerResult.shopItem.Price;

            if (buyerAvatarAddress == States.Instance.CurrentAvatarState.address)
            {
                var buyerAgentAddress = States.Instance.AgentState.address;
                var itemId = eval.Action.buyerResult.itemUsable.ItemId;

                LocalStateModifier.ModifyAgentGold(buyerAgentAddress, price);
                LocalStateModifier.RemoveItem(buyerAvatarAddress, itemId);
                LocalStateModifier.AddNewAttachmentMail(buyerAvatarAddress, itemId);
                RenderQuest(buyerAvatarAddress, eval.Action.buyerCompletedQuestIds);
                var format = LocalizationManager.Localize("NOTIFICATION_BUY_BUYER_COMPLETE");
                UI.Notification.Push(MailType.Auction, string.Format(format, eval.Action.buyerResult.itemUsable.GetLocalizedName()));
            }
            else
            {
                var sellerAvatarAddress = eval.Action.sellerAvatarAddress;
                var sellerAgentAddress = eval.Action.sellerAgentAddress;
                var itemId = eval.Action.sellerResult.itemUsable.ItemId;
                var gold = eval.Action.sellerResult.gold;

                LocalStateModifier.ModifyAgentGold(sellerAgentAddress, -gold);
                LocalStateModifier.AddNewAttachmentMail(sellerAvatarAddress, itemId);
                RenderQuest(sellerAvatarAddress, eval.Action.sellerCompletedQuestIds);
                var format = LocalizationManager.Localize("NOTIFICATION_BUY_SELLER_COMPLETE");
                var buyerName =
                    new AvatarState(
                            (Bencodex.Types.Dictionary) eval.OutputStates.GetState(eval.Action.buyerAvatarAddress))
                        .NameWithHash;
                var result = eval.Action.sellerResult;
                UI.Notification.Push(MailType.Auction, string.Format(format, buyerName, result.itemUsable.GetLocalizedName()));
            }

            UpdateAgentState(eval);
            UpdateCurrentAvatarState(eval);
        }
        
        private void ResponseHackAndSlash(ActionBase.ActionEvaluation<HackAndSlash> eval)
        {
            var battleResultWidget = Widget.Find<BattleResult>();

            battleResultWidget.BattleEndedSubject.Subscribe(_ =>
            {
                foreach (var questId in eval.Action.completedQuestIds)
                    LocalStateModifier.AddReceivableQuest(States.Instance.CurrentAvatarState.address, questId);
            });

            UpdateCurrentAvatarState(eval);
            UpdateWeeklyArenaState(eval);

            var actionFailPopup = Widget.Find<ActionFailPopup>();
            actionFailPopup.CloseCallback = null;
            actionFailPopup.Close();

            if (Widget.Find<QuestPreparation>().IsActive() &&
                Widget.Find<LoadingScreen>().IsActive())
            {
                Widget.Find<QuestPreparation>().GoToStage(eval);
            }
            else if (Widget.Find<BattleResult>().IsActive() &&
                Widget.Find<StageLoadingScreen>().IsActive())
            {
                Widget.Find<BattleResult>().NextStage(eval);
            }
        }

        private void ResponseQuestReward(ActionBase.ActionEvaluation<QuestReward> eval)
        {
            UpdateCurrentAvatarState(eval);
            var format = LocalizationManager.Localize("NOTIFICATION_QUEST_REWARD");
            var msg = string.Format(format, eval.Action.Result.GetName());
            UI.Notification.Push(MailType.System, msg);
        }

        private void ResponseItemEnhancement(ActionBase.ActionEvaluation<ItemEnhancement> eval)
        {
            var agentAddress = eval.InputContext.Signer;
            var avatarAddress = eval.Action.avatarAddress;
            var result = eval.Action.result;
            var itemUsable = result.itemUsable;

            LocalStateModifier.ModifyAgentGold(agentAddress, result.gold);
            LocalStateModifier.ModifyAvatarActionPoint(avatarAddress, result.actionPoint);
            LocalStateModifier.AddItem(avatarAddress, itemUsable.ItemId);
            foreach (var itemId in result.materialItemIdList)
            {
                LocalStateModifier.AddItem(avatarAddress, itemId);
            }
            LocalStateModifier.RemoveItem(avatarAddress, itemUsable.ItemId);
            LocalStateModifier.AddNewAttachmentMail(avatarAddress, itemUsable.ItemId);
            RenderQuest(avatarAddress, eval.Action.completedQuestIds);
            var format = LocalizationManager.Localize("NOTIFICATION_ITEM_ENHANCEMENT_COMPLETE");
            UI.Notification.Push(MailType.Workshop,
                string.Format(format, eval.Action.result.itemUsable.Data.GetLocalizedName()));
            UpdateAgentState(eval);
            UpdateCurrentAvatarState(eval);
        }

        private void ResponseRankingBattle(ActionBase.ActionEvaluation<RankingBattle> eval)
        {
            var weeklyArenaAddress = eval.Action.WeeklyArenaAddress;
            var avatarAddress = eval.Action.AvatarAddress;
            
            // fixme: 지금 개발 단계에서는 참가 액션이 분리되어 있지 않기 때문에, 참가할 때 깎은 골드를 더하지 못함.
            // LocalStateModifier.ModifyAgentGold(States.Instance.AgentState.address, GameConfig.ArenaActivationCostNCG);
            // fixme: 지금 개발 단계에서는 참가 액션이 분리되어 있지 않기 때문에, 참가할 때 더한 골드를 빼주지 못함.
            // LocalStateModifier.ModifyWeeklyArenaGold(-GameConfig.ArenaActivationCostNCG);
            LocalStateModifier.RemoveWeeklyArenaInfoActivator(weeklyArenaAddress, avatarAddress);
            
            UpdateAgentState(eval);
            UpdateCurrentAvatarState(eval);
            UpdateWeeklyArenaState(eval);

            var actionFailPopup = Widget.Find<ActionFailPopup>();
            actionFailPopup.CloseCallback = null;
            actionFailPopup.Close();

            Widget.Find<RankingBoard>().GoToStage(eval);
        }

        private void ResponseWeeklyArenaReward(ActionBase.ActionEvaluation<WeeklyArenaReward> eval)
        {
            var currentAgentState = States.Instance.AgentState;
            var agentState = eval.OutputStates.GetAgentState(currentAgentState.address);
            var gold = agentState.gold - currentAgentState.gold;
            UpdateAgentState(eval);
            Widget.Find<LoadingScreen>().Close();
            UI.Notification.Push(MailType.System, $"Get Arena Reward: {gold}");
        }

        public void RenderQuest(Address avatarAddress, IEnumerable<int> ids)
        {
            foreach (int id in ids)
            {
                LocalStateModifier.AddReceivableQuest(avatarAddress, id);

                var currentAvatarState = States.Instance.CurrentAvatarState;
                if (currentAvatarState.address == avatarAddress)
                {
                    var quest = currentAvatarState.questList.First(q => q.Id == id);
                    var rewardMap = quest.Reward.ItemMap;

                    foreach (var reward in rewardMap)
                    {
                        var materialRow = Game.Game.instance.TableSheets.MaterialItemSheet
                            .First(pair => pair.Key == reward.Key);

                        LocalStateModifier.RemoveItem(avatarAddress, materialRow.Value.ItemId, reward.Value);
                    }
                }
            }
        }
    }
}
