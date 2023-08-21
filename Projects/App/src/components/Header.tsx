/* eslint-disable @typescript-eslint/no-empty-function */
"use client";
// react
import React from "react";
// react-anchor-link-smooth-scroll-v2
import AnchorLink from "react-anchor-link-smooth-scroll";

// style
import styles from "./Header.module.scss";
// image
import logoImage from "../images/logo2.png";

export const scrollToTop = () => {
    window.scrollTo({
        top: 0,
        behavior: "auto",
    });
};

const logoBtnToggle = () => {
    scrollToTop();
};

export const Header = () => {
    return (
        <header className={styles.wrapper}>
            <div className={styles.container}>
                <div className={styles.logo}>
                    <a href="#" onClick={logoBtnToggle}>
                        <img src={logoImage} className={styles.logo_image} alt="logo" width={222} height={99} />
                    </a>
                </div>
                <div className={styles.menu}>
                    <AnchorLink className={styles.menu_item} href="#about">
                        概要と特徴
                    </AnchorLink>
                    <AnchorLink className={styles.menu_item} href="#use">
                        主な活用事例
                    </AnchorLink>
                    <AnchorLink className={styles.menu_item} href="#demo">
                        デモ
                    </AnchorLink>
                </div>
            </div>
        </header>
    );
};
