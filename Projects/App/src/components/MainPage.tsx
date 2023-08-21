// style
import styles from "./MainPage.module.scss";
// components
import { UnityCanvas } from "./UnityCanvas";
import { SoraCanvas } from "./SoraCanvas";
// images
import mainImage from "../images/main.jpg";
import headlineConciergeImage from "../images/headline_concier.jpg";
import headlineUseImage from "../images/headline_use.jpg";
import headlineDemoImage from "../images/headline_demo.jpg";

export const MainPage = () => {
    return (
        <>
            <main className={styles.main}>
                <div className={styles.content}>
                    <div className={styles.main_image}>
                        <img src={mainImage} alt="xr concierge" width={1280} height={600} />
                    </div>
                    <div className={styles.headline} id="about">
                        <img src={headlineConciergeImage} alt="xr concierge" width={1280} height={200} />
                        <p>
                            アバター経由でAI応対、有人応対も実施できるアバター接客システムです。
                            <br />
                            一次受け付けをChatBotで動作する「AI応答モード」で行い、難しい質問や対話が必用な業務は有人対応である「テレイグモード」に移行することで、本当に必要な時に、必要な場所へ人的稼働を割くことができます。
                        </p>
                    </div>
                    <div className={styles.headline} id="use">
                        <img src={headlineUseImage} alt="xr concierge" width={1280} height={200} />
                        <h3>窓口受付</h3>
                        <p>
                            オフィスやホテル、施設の総合案内などあらゆる窓口業務を自動/遠隔化できます。
                            <br />
                            人の応対が必要な時でも、窓口応対者はテレワークスペースから応対業務を行うことが可能です。
                        </p>
                        <h3>接客応対</h3>
                        <p>
                            商品紹介や販売、各種手続きを行うための接客業務を自動/遠隔化できます。難易度の高い対話が必要な応対業務において、本当に必要な時にだけ人の稼働を利用します。
                        </p>
                    </div>
                    <div className={styles.headline} id="demo">
                        <img src={headlineDemoImage} alt="xr concierge" width={1280} height={200} />
                        <div className={styles.demo_canvas}>
                            <UnityCanvas />
                        </div>
                    </div>
                </div>
            </main>
        </>
    );
};
