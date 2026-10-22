# TheOtherUs 职业速查(摘编自 mxyx.club wiki,2026-10-19 抓取)

> 来源:https://www.mxyx.club/archives/theotherus (沫夏悠轩の聚会)
> 用途:AI 职业适配卡的素材库。英文名以源码 RoleId 为准(见《00_源码导读笔记》§2.3 对照表)。
> 注意:wiki 有少量笔误(如"侠客"条目写的是赌怪说明,源码中 Vigilante=侠客/猜人型船员),以源码为准。

## 船员阵营(节选要点)

- **警长 Sheriff**:可击杀,误杀好人则自杀/双杀(按设置);副警长 Deputy 可铐人(被铐者禁用技能)。
- **侠客 Vigilante**:会议中猜职业,猜错死亡(船员侧赌怪)。
- **市长 Mayor**:票数=在场人数比例,可揭示身份换多票;有"存票"模式。
- **检察官 Prosecutor**:每局一次会议起诉,15 票;错放被投走则失去能力。
- **医生 Medic**:开局给一人护盾(ShieldedMurderAttempt),被刀者不死。
- **保镖 BodyGuard**:守护目标,替死。
- **工程师 Engineer**:原版工程师增强(钻管道)。
- **黑客 Hacker**:可查他人任务进度/生命(vitals/生物识别,次数限制)。
- **预言家 Seer / 先知 Prophet**:查验阵营/身份类信息位。
- **侦探 Detective**:查尸体获得凶手线索。
- **追踪者 Tracker / 告密者 Snitch**:定位类信息位。
- **老兵 Veteran**:开技能期间反杀来刀者。
- **换票师 Swapper**:会议交换两人票。
- **卧底 Spy**:知道内鬼是谁(不互通)。
- **星门缔造者 Portalmaker**:放传送门。
- **设陷师 Trapper/强化设陷师 TrapperPlus**:放陷阱。
- **平衡器 Balancer**:强制平衡票型。
- **狱长 Jailor**:监禁(禁言禁票)。
- **捕梦者 Dreamcatcher / 神谕 Oracle / 情报师 InfoSleuth / 先知**:信息/防护变体。
- **效颦者 Imitator**:临时借用其他船员职业能力(AI 读身份需过 Imitator 判定!)。

## 内鬼阵营(节选要点)

- **化形相关**:Camouflager(隐蔽者:全场同皮)、Mimic(模仿者:复制船员职业表)、Glitch(混沌:变形为指定玩家)。
- **处刑变体**:Undertaker(入殓师:拖尸)、Cleaner(清理者:毁尸)、Butcher(屠夫)、Professional(职业杀手:击杀无动画提示)。
- **吸血鬼 Vampire**:咬人延迟死亡,可放大蒜。
- **术士 Warlock**:诅咒目标,隔空处刑。**傀儡师 Marionette**:操控目标。
- **赏金猎人 BountyHunter**:有赏金目标,杀中刷新冷却。
- **恐怖分子 Terrorist**:死亡时爆炸。**炸弹狂 Bomber/Gunsmith/榴弹兵 Grenadier**:群体伤害类。
- **勒索者 Blackmailer**:禁言。**女巫 Witch**:诅咒禁 技能。**狱卒 Gaoler**:监禁。
- **忍者 Ninja / 悠悠球 Yoyo / 挖矿者 Miner / 管道工(Miner?)**:机动类。
- **抹除者 Eraser**:抹除职业能力。**赌徒 Gambler**:内鬼侧赌怪。**狼之主 WolfLord**。

## 中立阵营(节选要点)

- **无刀生存类**:Survivor(幸存者:背心挡刀)、Amnisiac(失忆者:捡职业)、PartTimer(打工仔)、Lawyer(律师:绑定委托人)、Pursuer(起诉人)。
- **会议型**:Jester(小丑:被投走胜)、Executioner(处刑者:目标被投走胜)、Doomsayer(末日预言家:猜 3 人身份胜,15 发)、Witness(污点证人)、Thief(小偷)。
- **带刀杀手**:Jackal(豺狼:杀+造跟班 Sidekick)、Werewolf(月下狼人)、Juggernaut(天启:越杀越强)、Arsonist(纵火狂:浇油点火)、Pelican(鹈鹕:吞人)、Phantom(隐身人)、Infected(感染者:同化)、Pavlovs(巴甫洛夫主仆)、Akujo(恶女)、BandLeader(乐队主唱:拉帮)、SchrodingersCat(薛定谔的猫:变阵)。

## 附加能力(Modifier,可与主职业叠加)

红狼侧:Assassin(刺客:会议猜人)、Disperser(分散者)、Vortox(迷乱旋涡)、LastImpostor、入殓师、绝境者;
船员侧:Torch(火炬:夜视)、Sunglasses(太阳镜)、Multitasker(多线程:任务加速)、管道工程师、反骨、交换师 Shifter(换职业);
通用:Bait(诱饵:尸体举报者暴露)、Aftermath(余波)、Flash(闪电侠)、Mini(小孩:成长期不可被杀)、Giant(巨人)、VIP、Indomitable(不屈)、ButtonBarry(执钮人)、Chameleon(变色龙:静止渐隐)、Radar(雷达)、Tiebreaker(破平者)、Tunneler、Bloody(溅血者)、Blind/Watcher、Cursed(被诅咒)、酒鬼、破平者、通讯兵、掘墓人。

## 幽灵职业(死后)

GhostEngineer(灵魂工程师:帮做任务)、Specter(怨灵)、Clog(阻挠鬼)、Poltergeist(捣蛋鬼)。

## 游戏模式

经典 / 赌怪模式(GuesserGM,开局全员可猜)/ 轮抽选角(RoleDraft)/ 匿名模式(Anonymous)。
**赌怪风险**:会议中报真实身份可能被猜杀 → claim_role 策略必须考虑(AI 会议模块已设计该字段)。
