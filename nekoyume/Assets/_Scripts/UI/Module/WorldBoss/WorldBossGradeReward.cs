using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.BlockChain;
using Nekoyume.Helper;
using Nekoyume.L10n;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI.Module.WorldBoss
{
    using UniRx;

    public class WorldBossGradeReward : WorldBossRewardItem
    {
        [SerializeField]
        private TextMeshProUGUI myBestRecordText;

        [SerializeField]
        private List<WorldBossGradeRewardItem> items;

        [SerializeField]
        private ConditionalButton claimButton;

        [SerializeField]
        private Image startGaugeImage;

        [SerializeField]
        private Image middleGaugeImage;

        private bool _canReceive;

        private void Start()
        {
            claimButton.OnSubmitSubject
                .Subscribe(_ => ClaimRaidReward())
                .AddTo(gameObject);

            claimButton.OnClickDisabledSubject
                .Subscribe(_ =>
                {
                }).AddTo(gameObject);

        }

        private void OnEnable()
        {
            claimButton.Interactable = _canReceive;
        }

        public override void Reset()
        {

        }

        public void Set(RaiderState raiderState, int raidId)
        {
            if (!WorldBossFrontHelper.TryGetRaid(raidId, out var row))
            {
                return;
            }

            var rewardSheet = Game.Game.instance.TableSheets.WorldBossRankRewardSheet;
            var rows = rewardSheet.Values.Where(x => x.BossId.Equals(row.BossId)).ToList();
            if (!rows.Any())
            {
                return;
            }

            if (!Game.Game.instance.TableSheets.WorldBossCharacterSheet
                .TryGetValue(row.BossId, out var characterRow))
            {
                return;
            }

            Widget.Find<WorldBossRewardPopup>().CachingInformation(raiderState, row.BossId);
            var latestRewardRank = raiderState?.LatestRewardRank ?? 0;
            var highScore = raiderState?.HighScore ?? 0;
            var currentRank = WorldBossHelper.CalculateRank(characterRow, highScore);
            _canReceive = latestRewardRank < currentRank;
            UpdateItems(characterRow, rows, latestRewardRank, currentRank);
            UpdateGauges(characterRow, highScore, currentRank);
            UpdateRecord(highScore);
        }

        private void ClaimRaidReward()
        {
            Widget.Find<GrayLoadingScreen>().Show("UI_LOADING_REWARD", true, 0.7f);
            ActionManager.Instance.ClaimRaidReward();
            _canReceive = false;
            claimButton.Interactable = _canReceive;
        }

        private void UpdateItems(
            WorldBossCharacterSheet.Row bossRow,
            IReadOnlyList<WorldBossRankRewardSheet.Row> rows,
            int latestRewardRank,
            int currentRank)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var score = WorldBossFrontHelper.GetScoreInRank(i + 1, bossRow);
                items[i].Set(score, rows[i].Rune, rows[i].Crystal);

                if (i + 1 <= latestRewardRank)
                {
                    items[i].SetStatus(WorldBossGradeRewardItem.Status.Received);
                }
                else
                {
                    items[i].SetStatus(i < currentRank
                        ? WorldBossGradeRewardItem.Status.Active
                        : WorldBossGradeRewardItem.Status.Normal);
                }
            }
        }

        private void UpdateGauges(WorldBossCharacterSheet.Row bossRow, int highScore, int currentRank)
        {
            switch (currentRank)
            {
                case (int)WorldBossGrade.None:
                    var d = WorldBossFrontHelper.GetScoreInRank((int)WorldBossGrade.D, bossRow);
                    startGaugeImage.fillAmount = d > 0 ? highScore / (float)d : 0;
                    middleGaugeImage.fillAmount = 0;
                    break;
                case (int)WorldBossGrade.S:
                    startGaugeImage.fillAmount = 1;
                    middleGaugeImage.fillAmount = 1;
                    break;
                default:
                    var curRankScore = WorldBossFrontHelper.GetScoreInRank(currentRank, bossRow);
                    var nextRankScore = WorldBossFrontHelper.GetScoreInRank(currentRank + 1, bossRow);
                    var max = nextRankScore - curRankScore;
                    var cur = highScore - curRankScore;
                    startGaugeImage.fillAmount = 1;
                    middleGaugeImage.fillAmount = (0.25f * (currentRank - 1)) + (0.25f * (cur / (float)max));
                    break;
            }
        }

        private void UpdateRecord(int highScore)
        {
            myBestRecordText.text = $"{highScore:#,0}";
        }
    }
}
