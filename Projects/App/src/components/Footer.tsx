/* eslint-disable @typescript-eslint/no-empty-function */
// "use client";
// react
import React from "react";
// style
import styles from "./Footer.module.scss";

export const scrollToTop = () => {
    window.scrollTo({
        top: 0,
        behavior: "auto",
    });
};

export const Footer = () => {
    return (
        <footer className={styles.wrapper}>
            <div className={styles.container}>
                <p>©XR Concierge Team</p>
            </div>
        </footer>
    );
};
